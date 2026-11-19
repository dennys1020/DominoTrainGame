using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DominoTrainGame.Client.View.Behaviors;

public sealed class TileDragAdorner : Adorner
{
    private const double DraggedTileSourceOpacity = 0.4;

    private readonly VisualBrush _tileBrush;
    private readonly UIElement _tileHolder;
    private readonly UIElement _pointerSurface;
    private readonly Size _tileSize;
    private readonly Vector _grabOffset;

    private Point _tileOrigin;

    public TileDragAdorner(UIElement dragSurface, FrameworkElement tileElement, Point grabPoint)
        : base(dragSurface)
    {
        Rect tileBounds = tileElement
            .TransformToVisual(dragSurface)
            .TransformBounds(new Rect(tileElement.RenderSize));

        _tileBrush = new VisualBrush(tileElement);
        _tileHolder = VisualTreeHelper.GetParent(tileElement) as UIElement ?? tileElement;
        _pointerSurface = Window.GetWindow(dragSurface) ?? dragSurface;
        _tileSize = tileBounds.Size;
        _grabOffset = grabPoint - tileBounds.TopLeft;
        _tileOrigin = tileBounds.TopLeft;
        IsHitTestVisible = false;
    }

    public void Show(AdornerLayer adornerLayer)
    {
        adornerLayer.Add(this);
        _tileHolder.Opacity = DraggedTileSourceOpacity;
        _pointerSurface.AllowDrop = true;
        _pointerSurface.PreviewDragOver += FollowPointer;
        _pointerSurface.DragEnter += RejectDropOutsideTrains;
        _pointerSurface.DragOver += RejectDropOutsideTrains;
    }

    public void Hide(AdornerLayer adornerLayer)
    {
        _pointerSurface.PreviewDragOver -= FollowPointer;
        _pointerSurface.DragEnter -= RejectDropOutsideTrains;
        _pointerSurface.DragOver -= RejectDropOutsideTrains;
        _pointerSurface.ClearValue(AllowDropProperty);
        _tileHolder.ClearValue(OpacityProperty);
        adornerLayer.Remove(this);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        drawingContext.DrawRectangle(_tileBrush, null, new Rect(_tileOrigin, _tileSize));
    }

    private static void RejectDropOutsideTrains(object pointerSurface, DragEventArgs eventArgs)
    {
        eventArgs.Effects = DragDropEffects.None;
        eventArgs.Handled = true;
    }

    private void FollowPointer(object pointerSurface, DragEventArgs eventArgs)
    {
        _tileOrigin = eventArgs.GetPosition(AdornedElement) - _grabOffset;
        InvalidateVisual();
    }
}
