﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace LogGrokCore.AvalonDockExtensions;

public class ObservableCollectionFactoryLink<TTarget> where TTarget : class
{
    public IList SourceCollection { get; }

    public ObservableCollection<TTarget> TargetCollection { get; }

    public Func<object, TTarget> Factory { get; }
        
    private readonly Dictionary<object, TTarget> _sourceToTargetMapping = new();
        
    public ObservableCollectionFactoryLink(IList source, ObservableCollection<TTarget> target, 
        Func<object,TTarget> factory)
    {
        SourceCollection = source;
        TargetCollection = target;
        Factory = factory;

        TargetCollection.Clear();
        foreach (var s in SourceCollection.Cast<object>().Where(item => item != null))
        {
            var t = Factory(s);
            _sourceToTargetMapping[s] = t;
            TargetCollection.Add(t);
        }

        void SyncSourceChanges(IList source, ObservableCollection<TTarget> target)
        {
            bool haveSource(TTarget trgt)
            {
                var src = _sourceToTargetMapping.First(k => object.ReferenceEquals(k.Value, trgt)).Key;
                return source.Contains(src);
            }

            var itemsToAdd = source.Cast<object>().Except(_sourceToTargetMapping.Keys).Select(s => (s, Factory(s))).ToList();
            var itemsToRemove = target.Where(t => !haveSource(t)).ToList();

            foreach (var i in itemsToAdd)
            {
                var (s, t) = i;
                _sourceToTargetMapping[s] = t;
                target.Add(t);
            }

            foreach (var i in itemsToRemove)
            {
                target.Remove(i);

                if (SourceFromTarget(i) is object s)
                { 
                    _sourceToTargetMapping.Remove(s);
                }
            }
        }
        

        void SyncTargetChanges(IList source, ObservableCollection<TTarget> target)
        {
            var itemsToSave = _sourceToTargetMapping.Where(k => target.Contains(k.Value)).Select(k => k.Key).ToList();
            var itemsToRemove = source.Cast<object>().Except(itemsToSave).ToList();

            foreach(var i in itemsToRemove)
            {
                source.Remove(i);
                _sourceToTargetMapping.Remove(i);
            }
        }

        ((INotifyCollectionChanged)SourceCollection).CollectionChanged += (_, __) => SyncSourceChanges(SourceCollection, TargetCollection);
        TargetCollection.CollectionChanged += (_, __) => SyncTargetChanges(SourceCollection, TargetCollection);
    }

    public object? SourceFromTarget(TTarget target)
    {
        var result = _sourceToTargetMapping.FirstOrDefault(kv => ReferenceEquals(kv.Value, target));
        return result.Equals(default(KeyValuePair<object, TTarget>)) ? null : result.Key;
    }

    public TTarget? TargetFromSource(object source)
    {
        if (source == null || !_sourceToTargetMapping.ContainsKey(source))
            return null;
        return _sourceToTargetMapping[source];
    }
}