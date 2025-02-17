﻿// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Client.Message;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Utility;

namespace Content.Client.SS220.PipePainter.UI;

[GenerateTypedNameReferences]
public sealed partial class PipePainterWindow : DefaultWindow
{
    [Dependency] private readonly IEntitySystemManager _sysMan = default!;
    private readonly SpriteSystem _spriteSystem;

    public Action<string>? OnColorPicked;

    private Dictionary<string, Color> _currentPalette = new();
    private Dictionary<string, ItemList.Item> _itemIndex = new();
    private const string ColorLocKeyPrefix = "pipe-painter-color-";

    private readonly SpriteSpecifier _colorEntryIconTexture = new SpriteSpecifier.Rsi(
        new ResPath("Structures/Piping/Atmospherics/pipe.rsi"),
        "pipeStraight");

    public PipePainterWindow()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
        _spriteSystem = _sysMan.GetEntitySystem<SpriteSystem>();
    }

    private void OnColorSelected(ItemList.ItemListSelectedEventArgs args)
    {
        // y tf no metadata in ItemList.ItemListSelectedEventArgs reeeeeeeeeeeee
        var data = args.ItemList[args.ItemIndex].Metadata;
        if (data is string str)
            OnColorPicked?.Invoke(str);
    }

    private static string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var output = input[0].ToString().ToUpper();
        if (input.Length > 1)
            output += input[1..];

        return output;
    }

    private static string GetColorLocString(string? colorKey)
    {
        if (string.IsNullOrEmpty(colorKey))
            return Loc.GetString("pipe-painter-no-color-selected");

        var locKey = ColorLocKeyPrefix + colorKey;
        if (!Loc.TryGetString(locKey, out var locString))
            locString = colorKey;

        return locString;
    }

    private void UpdateSelectedTextLabel(string? selected)
    {
        var colorLabel = GetColorLocString(selected);

        if(string.IsNullOrEmpty(selected) || !_currentPalette.TryGetValue(selected, out var color))
            color = Color.White;

        SelectedColorLabel.SetMarkup($"[color={color.ToHexNoAlpha()}]{colorLabel}[/color]");
    }

    public void Populate(Dictionary<string, Color> palette, string? selected)
    {
        // Only clear if the entries change. Otherwise the list would "jump" after selecting an item
        if (!_currentPalette.Equals(palette))
        {
            _currentPalette = palette;
            _itemIndex = new Dictionary<string, ItemList.Item>();
            ColorList.Clear();
            foreach (var entry in palette)
            {
                var locString = CapitalizeFirstLetter(GetColorLocString(entry.Key));
                var item = ColorList.AddItem(locString, _spriteSystem.Frame0(_colorEntryIconTexture));
                item.IconModulate = entry.Value;
                item.Metadata = entry.Key;

                _itemIndex.Add(entry.Key, item);
            }
        }

        UpdateSelectedTextLabel(selected);

        if(selected is null)
            return;

        if(_itemIndex.TryGetValue(selected, out var selectedItem))
        {
            // Disable event so we don't send a new event for pre-selected entry and end up in a loop
            ColorList.OnItemSelected -= OnColorSelected;
            selectedItem.Selected = true;
            ColorList.OnItemSelected += OnColorSelected;
        }
    }
}
