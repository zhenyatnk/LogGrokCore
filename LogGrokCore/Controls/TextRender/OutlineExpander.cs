﻿using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace LogGrokCore.Controls.TextRender;

public enum OutlineExpanderState
{
    Collapsed,
    ExpandedUpper,
    ExpandedLower
}

public class OutlineExpander : ButtonBase
{
    public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
        nameof(State), typeof(OutlineExpanderState),
        typeof(OutlineExpander), 
        new FrameworkPropertyMetadata(default(OutlineExpanderState), 
            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

    public OutlineExpander()
    {
        Cursor = Cursors.Hand;
    }

    static OutlineExpander()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(OutlineExpander), new FrameworkPropertyMetadata(null));
    }
    
    public OutlineExpanderState State
    {
        get => (OutlineExpanderState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public Expandable? Expandable { get; set; }
    
    protected override void OnClick()
    {
        Expandable?.Toggle();
        base.OnClick();
    }

    protected override Size MeasureOverride(Size constraint)
    {
        var sz = State switch
        {
            OutlineExpanderState.Collapsed => new Size(1, 1),
            OutlineExpanderState.ExpandedLower => new Size(3, 4),
            OutlineExpanderState.ExpandedUpper => new Size(3, 4),
            _ => throw new ArgumentOutOfRangeException($"Unexpected State Value: {State}.")
        };

        var normalizedByWidth = new Size(constraint.Width, constraint.Width / sz.Width * sz.Height);
        
        _currentSize = normalizedByWidth.Height <= constraint.Height
            ? normalizedByWidth
            : new Size(constraint.Height / sz.Height * sz.Width, constraint.Height);
        return _currentSize;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        drawingContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Transparent, 1), new Rect(0, 0, ActualWidth, ActualHeight));
        
        var width = _currentSize.Width;
        var height = _currentSize.Height;

        var xOffset = (ActualWidth - width) / 2;
        var yOffset = (ActualHeight - height) / 2;

        var pen = new Pen(Foreground, thickness: 1);
        
        var pixelsPerDip = (float)VisualTreeHelper.GetDpi(this).PixelsPerDip;

        double Round(double x)
        {
            return Math.Round(x * pixelsPerDip,
                MidpointRounding.ToEven) / pixelsPerDip;
        }

        void DrawLine(double x1, double y1, double x2, double y2)
        {
            drawingContext.DrawLine(pen, new Point(Round(x1 + xOffset),Round(y1 + yOffset)), 
                new Point(Round(x2 + xOffset), Round(y2 + yOffset)));
        }

        var halfPixel = 0.5;
        var guidelines = new GuidelineSet();
        guidelines.GuidelinesX.Add(0 + halfPixel);
        guidelines.GuidelinesX.Add(width + halfPixel);
        guidelines.GuidelinesX.Add(Round(width/2) + halfPixel);
        guidelines.GuidelinesX.Add(Round(width/6) + halfPixel);
        guidelines.GuidelinesX.Add(Round(width*5/6) + halfPixel);
        guidelines.GuidelinesY.Add(0 + halfPixel);
        
        switch (State)
        {
            case OutlineExpanderState.Collapsed:
                guidelines.GuidelinesY.Add(height - halfPixel );
                drawingContext.PushGuidelineSet(guidelines);
                DrawLine(0, 0, width, 0);
                DrawLine(width, 0, width, height);
                DrawLine(width, height, 0, height );
                DrawLine(0, height , 0, 0 );
                DrawLine(width/2, height/6, width/2, height * 5/6);
                DrawLine(width/6, height/2, width*5/6, height/2);
                break;
            case OutlineExpanderState.ExpandedUpper:
                guidelines.GuidelinesY.Add(Round(height/2 - height/12) + halfPixel);
                guidelines.GuidelinesY.Add(height + halfPixel );
                drawingContext.PushGuidelineSet(guidelines);
                DrawLine(0, 0, width, 0);
                DrawLine(width, 0, width, height * 7.5 / 12);
                DrawLine(width, height * 7.5 / 12, width / 2, height);
                DrawLine(width / 2, height, 0, height * 7.5 / 12 );
                DrawLine(0, height * 7.5 / 12, 0, 0);
                DrawLine(width/6, height/2 - height/12, width*5/6, height/2 - height/12);
                break;
            case OutlineExpanderState.ExpandedLower:
                guidelines.GuidelinesY.Add(Round(height/2 + height/12) + halfPixel);
                guidelines.GuidelinesY.Add(height + halfPixel );
                drawingContext.PushGuidelineSet(guidelines);
                DrawLine(0, height, width, height);
                DrawLine(width, height, width, height * 4.5 / 12);
                DrawLine(width, height * 4.5 / 12, width / 2, 0);
                DrawLine( width / 2,  0, 0, height * 4.5 / 12);
                DrawLine(  0, height * 4.5 / 12, 0, height);
                DrawLine(width/6, height/2 + height/12, width*5/6, height/2 + height/12);
                break;
            default:
                DrawLine(width/6, height/2, width*5/6, height/2);
                break;
        }
        
        drawingContext.Pop();
    }

    private Size _currentSize;
}