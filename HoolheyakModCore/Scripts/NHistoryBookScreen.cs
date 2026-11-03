using System;
using System.Linq;
using Godot;
using HoolheyakMod.Code.Relics;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;

namespace HoolheyakMod.Scripts;

public partial class NHistoryBookScreen : Control, IOverlayScreen
{
    private BeautifulHistoryBook _book = null!;
    private Button _closeButton = null!;
    public NetScreenType ScreenType => NetScreenType.Compendium;
    public bool UseSharedBackstop => true;
    public Control DefaultFocusedControl => _closeButton;

    public static void Show(BeautifulHistoryBook book)
    {
        var stack = NOverlayStack.Instance;
        if (stack == null || stack.Peek() is NHistoryBookScreen) return;
        stack.Push(new NHistoryBookScreen { _book = book });
    }

    internal static string EnemyName(string entry)
    {
        try
        {
            return ModelDb.GetByIdOrNull<MonsterModel>(new ModelId("MONSTER", entry))?.Title.GetFormattedText() ?? entry;
        }
        catch { return entry; }
    }

    private LocString Text(string suffix) => new("relics", _book.Id.Entry + "." + suffix);

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        var font = new SystemFont { FontNames = ["Noto Sans CJK SC", "Microsoft YaHei", "sans-serif"] };
        var theme = new Theme { DefaultFont = font, DefaultFontSize = 26 };
        Theme = theme;
        var panel = new PanelContainer { Name = "HistoryPanel", MouseFilter = MouseFilterEnum.Stop };
        AddChild(panel);
        panel.AnchorLeft = 0.18f; panel.AnchorRight = 0.82f;
        panel.AnchorTop = 0.12f; panel.AnchorBottom = 0.88f;
        var style = new StyleBoxFlat {
            BgColor = new Color(0.025f,0.055f,0.075f,0.98f), BorderColor = new Color(0.23f,0.67f,0.73f),
            BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2,
            CornerRadiusTopLeft = 12, CornerRadiusTopRight = 12, CornerRadiusBottomLeft = 12, CornerRadiusBottomRight = 12,
            ContentMarginLeft = 28, ContentMarginRight = 28, ContentMarginTop = 24, ContentMarginBottom = 24
        };
        panel.AddThemeStyleboxOverride("panel", style);
        var content = new VBoxContainer { Name = "Contents" };content.AddThemeConstantOverride("separation",16);panel.AddChild(content);
        var ids = _book.GetRecordedEnemyIds();
        var heading = Text("list_title"); heading.Add("Count",ids.Count);
        var title = new Label { Text = heading.GetFormattedText(), HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size",34);title.AddThemeColorOverride("font_color",new Color(0.94f,0.8f,0.42f));content.AddChild(title);
        var body = new RichTextLabel {
            Name = "EnemyList", SizeFlagsVertical = SizeFlags.ExpandFill, ScrollActive = true,
            BbcodeEnabled = false, Text = ids.Count == 0 ? Text("list_empty").GetFormattedText() :
                string.Join("\n",ids.Select(EnemyName).OrderBy(name=>name,StringComparer.CurrentCulture).Select((name,index)=>$"{index+1}. {name}"))
        };
        content.AddChild(body);
        var hint = new Label { Text = Text("list_hint").GetFormattedText(), AutowrapMode = TextServer.AutowrapMode.WordSmart, HorizontalAlignment = HorizontalAlignment.Center };
        hint.AddThemeFontSizeOverride("font_size",20);content.AddChild(hint);
        _closeButton = new Button { Name = "Close", Text = Text("list_close").GetFormattedText(), CustomMinimumSize = new Vector2(0,48) };
        _closeButton.Pressed += Close;content.AddChild(_closeButton);
    }

    public void Close()
    {
        if (NOverlayStack.Instance is { } stack) stack.Remove(this);
        else QueueFree();
    }
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left or MouseButton.Right })
        { Close(); AcceptEvent(); }
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape })
        { GetViewport().SetInputAsHandled(); Close(); }
    }
    public void AfterOverlayOpened() { }
    public void AfterOverlayClosed() => QueueFree();
    public void AfterOverlayShown() => Show();
    public void AfterOverlayHidden() => Hide();
}
