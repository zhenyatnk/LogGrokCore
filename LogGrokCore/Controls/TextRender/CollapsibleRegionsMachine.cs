﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogGrokCore.Controls.TextRender;

public abstract class Outline
{
    public static Outline None => new None();
}

public sealed class None : Outline
{
}

public abstract class Expandable : Outline
{
    private readonly int _index;
    private readonly Action<int> _toggle;
 
    protected Expandable(int index, Action<int> toggle)
    {
        _index = index;
        _toggle = toggle;
    }

    public void Toggle()
    {
        _toggle(_index);
    }   
}

public sealed class ExpandedUpper : Expandable
{
    public ExpandedUpper(int index, Action<int> toggle) 
        : base(index, toggle)
    {
    }
}
public sealed class ExpandedLower : Expandable
{
    public ExpandedLower(int index, Action<int> toggle) 
        : base(index, toggle)
    {
    }
}

public sealed class Collapsed : Expandable
{
    public Collapsed(int index, Action<int> toggle) : base(index, toggle)
    {
    }
}

public class CollapsibleRegionsMachine : IEnumerable<(Outline outline, int index)>
{
    private readonly int _lineCount;
    private readonly Func<HashSet<int>?> _collapsedLineIndicesGetter;
    public (Outline, int) this[int index] => _regions[index];
    public int LineCount => _regions.Count;

    private readonly List<(Outline, int)> _regions;
    private readonly (bool isCollapsed, int start, int length)[] _collapsibleRegions;
    private readonly Action<int> _toggleAction;

    private HashSet<int> _collapsedLines = new ();

    public CollapsibleRegionsMachine(int lineCount, (int start, int length)[] collapsibleRegions,
        Func<HashSet<int>?> collapsedLineIndicesGetter)
    {
        _lineCount = lineCount;
        _collapsedLineIndicesGetter = collapsedLineIndicesGetter;
        var collapsedLines = collapsedLineIndicesGetter();
        if (collapsedLines != null)
        {
            _collapsedLines = collapsedLines;
        }
        
        _collapsibleRegions = collapsibleRegions.Select(region 
            => (_collapsedLines.Contains(region.start), region.start, region.length)).ToArray();
        _regions = new List<(Outline, int)>(collapsibleRegions.Length);
        
        _toggleAction = Toggle;
        Update();
    }

    public void UpdateCollapsedLines(HashSet<int> collapsedLines)
    {
        _collapsedLines = collapsedLines;
        for (var i = 0; i < _collapsibleRegions.Length; i++)
        {
            var (_, start, length) = _collapsibleRegions[i];
            _collapsibleRegions[i] = (_collapsedLines.Contains(start), start, length);
        }

        Update();
    }

    public bool IsCollapsed(int sourceLineNumber)
    {
        return _collapsedLines.Contains(sourceLineNumber);
    }

    public event Action? Changed;

    private void Update()
    {
        var collapsedLines = _collapsedLineIndicesGetter();
        if (collapsedLines != null)
        {
            _collapsedLines = collapsedLines;
        }
        
        _regions.Clear();
        _collapsedLines.Clear();
        var starts = _collapsibleRegions.ToDictionary(r => r.start, r => r);
        var ends = _collapsibleRegions.ToDictionary(r => r.start + r.length - 1, r => r);
        
        for (var i = 0; i < _lineCount; i++)
        {
            if (!starts.TryGetValue(i, out var rangeStart))
            {
                rangeStart = default;
            }
            
            if (!ends.TryGetValue(i, out var rangeEnd))
            {
                rangeEnd = default;
            }
            
            var outline = (rangeStart, rangeEnd) switch
            {
                ((false,0,0), (false,0,0)) => Outline.None,
                ((false, _, _), (false, 0,0)) => new ExpandedUpper(i, _toggleAction),
                ((false,0,0), (false, _, _)) => new ExpandedLower(i, _toggleAction),
                ((true, _, _), (false, 0, 0)) => new Collapsed(i, _toggleAction),
                _ => throw new InvalidOperationException()
            };
            
            _regions.Add((outline, i));

            if (outline is Collapsed)
            {
                i += rangeStart.length - 1;
            }
        }
        
        foreach (var (isCollapsed, start,_) in _collapsibleRegions)
        {
            if (isCollapsed)
                _collapsedLines.Add(start);
        }
        
        Changed?.Invoke();
    }

    private void Toggle(int index)
    {
        for (var i = 0; i < _collapsibleRegions.Length; i++)
        {
            var (isCollapsed, start, length) = _collapsibleRegions[i];
            if (start != index && start + length - 1 != index) continue;
            _collapsibleRegions[i] = (!isCollapsed, start, length);
            Update();
            return;
        } 
    }

    public void ExpandRecursively()
    {
        for (var i = 0; i < _collapsibleRegions.Length; i++)
        {
            var (_, start, length) = _collapsibleRegions[i];
            _collapsibleRegions[i] = (false, start, length);
        }
        Update();
    }

    public bool HasCollapsedRegions() => _collapsibleRegions.Any(c => c.isCollapsed);

    public void CollapseRecursively()
    {
        for (var i = 0; i < _collapsibleRegions.Length; i++)
        {
            var (_, start, length) = _collapsibleRegions[i];
            _collapsibleRegions[i] = (true, start, length);
        }
        Update();
    }

    public bool HasExpandedRegions() => _collapsibleRegions.Any(c => !c.isCollapsed);

    public IEnumerator<(Outline, int)> GetEnumerator()
    {
        for (var i = 0; i < LineCount; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}