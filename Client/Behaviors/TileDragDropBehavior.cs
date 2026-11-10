using System.Windows;
using System.Windows.Input;
using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.View.Behaviors;

public static class TileDragDropBehavior
{
    private const string TileIdFormat = "DominoTrainGame.TileId";
    private const string IsTileSourcePropertyName = "IsTileSource";
    private const string DropTileCommandPropertyName = "DropTileCommand";
    private const string IsDropAllowedPropertyName = "IsDropAllowed";
    private const string IsDragOverPropertyName = "IsDragOver";

    public static readonly DependencyProperty IsTileSourceProperty;
    public static readonly DependencyProperty DropTileCommandProperty;
    public static readonly DependencyProperty IsDropAllowedProperty;
    public static readonly DependencyProperty IsDragOverProperty;

    static TileDragDropBehavior()
    {
        IsTileSourceProperty = DependencyProperty.RegisterAttached(
            IsTileSourcePropertyName,
            typeof(bool),
            typeof(TileDragDropBehavior),
            new PropertyMetadata(false, OnIsTileSourceChanged));
        DropTileCommandProperty = DependencyProperty.RegisterAttached(
            DropTileCommandPropertyName,
            typeof(ICommand),
            typeof(TileDragDropBehavior),
            new PropertyMetadata(null, OnDropTileCommandChanged));
        IsDropAllowedProperty = DependencyProperty.RegisterAttached(
            IsDropAllowedPropertyName,
            typeof(bool),
            typeof(TileDragDropBehavior),
            new PropertyMetadata(false));
        IsDragOverProperty = DependencyProperty.RegisterAttached(
            IsDragOverPropertyName,
            typeof(bool),
            typeof(TileDragDropBehavior),
            new PropertyMetadata(false));
    }

    public static bool GetIsTileSource(DependencyObject target)
    {
        bool isTileSource = (bool)target.GetValue(IsTileSourceProperty);
        return isTileSource;
    }

    public static void SetIsTileSource(DependencyObject target, bool value)
    {
        target.SetValue(IsTileSourceProperty, value);
    }

    public static ICommand GetDropTileCommand(DependencyObject target)
    {
        ICommand dropTileCommand = (ICommand)target.GetValue(DropTileCommandProperty);
        return dropTileCommand;
    }

    public static void SetDropTileCommand(DependencyObject target, ICommand value)
    {
        target.SetValue(DropTileCommandProperty, value);
    }

    public static bool GetIsDropAllowed(DependencyObject target)
    {
        bool isDropAllowed = (bool)target.GetValue(IsDropAllowedProperty);
        return isDropAllowed;
    }

    public static void SetIsDropAllowed(DependencyObject target, bool value)
    {
        target.SetValue(IsDropAllowedProperty, value);
    }

    public static bool GetIsDragOver(DependencyObject target)
    {
        bool isDragOver = (bool)target.GetValue(IsDragOverProperty);
        return isDragOver;
    }

    public static void SetIsDragOver(DependencyObject target, bool value)
    {
        target.SetValue(IsDragOverProperty, value);
    }

    private static void OnIsTileSourceChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        if (target is UIElement tileElement)
        {
            bool isTileSource = (bool)e.NewValue;

            tileElement.MouseMove -= OnTileMouseMove;

            if (isTileSource)
            {
                tileElement.MouseMove += OnTileMouseMove;
            }
        }
    }

    private static void OnDropTileCommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        if (target is UIElement train)
        {
            bool hasCommand = e.NewValue is ICommand;

            train.DragEnter -= OnTrainDragOver;
            train.DragOver -= OnTrainDragOver;
            train.DragLeave -= OnTrainDragLeave;
            train.Drop -= OnTrainDrop;
            train.AllowDrop = hasCommand;

            if (hasCommand)
            {
                train.DragEnter += OnTrainDragOver;
                train.DragOver += OnTrainDragOver;
                train.DragLeave += OnTrainDragLeave;
                train.Drop += OnTrainDrop;
            }
        }
    }

    private static void OnTileMouseMove(object sender, MouseEventArgs e)
    {
        FrameworkElement tileElement = (FrameworkElement)sender;
        bool isDragging = e.LeftButton == MouseButtonState.Pressed && tileElement.DataContext is TileSummary;

        if (isDragging)
        {
            TileSummary tile = (TileSummary)tileElement.DataContext;
            DataObject draggedTile = new DataObject(TileIdFormat, tile.TileId);

            DragDrop.DoDragDrop(tileElement, draggedTile, DragDropEffects.Move);
        }
    }

    private static void OnTrainDragOver(object sender, DragEventArgs e)
    {
        DependencyObject train = (DependencyObject)sender;
        bool canDrop = CanAcceptTile(train, e.Data);

        e.Effects = canDrop ? DragDropEffects.Move : DragDropEffects.None;
        SetIsDragOver(train, canDrop);
        e.Handled = true;
    }

    private static void OnTrainDragLeave(object sender, DragEventArgs e)
    {
        SetIsDragOver((DependencyObject)sender, false);
    }

    private static void OnTrainDrop(object sender, DragEventArgs e)
    {
        DependencyObject train = (DependencyObject)sender;
        ICommand dropTileCommand = GetDropTileCommand(train);
        object tileId = e.Data.GetData(TileIdFormat);
        bool canDrop = CanAcceptTile(train, e.Data) && dropTileCommand.CanExecute(tileId);

        SetIsDragOver(train, false);

        if (canDrop)
        {
            dropTileCommand.Execute(tileId);
        }

        e.Handled = true;
    }

    private static bool CanAcceptTile(DependencyObject train, IDataObject data)
    {
        bool hasTile = data.GetDataPresent(TileIdFormat);
        bool canAccept = hasTile && GetIsDropAllowed(train);
        return canAccept;
    }
}
