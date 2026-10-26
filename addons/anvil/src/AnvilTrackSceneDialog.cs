#if TOOLS
using Godot;

namespace Anvil;

public partial class AnvilTrackSceneDialog : ConfirmationDialog
{
    private const int ContentWidth = 440;

    private LineEdit _sceneField;
    private Label _outputNameLabel;
    private Label _scriptNote;
    private LineEdit _outputNameField;
    private CheckBox _flattenField;

    private string _scenePath;
    private AnvilSceneRule _existingRule;
    private bool _nameRequired;

    public override void _Ready()
    {
        Title = "Track Scene with Anvil";
        Unresizable = false;
        Exclusive = true;

        var root = new VBoxContainer { CustomMinimumSize = new Vector2(ContentWidth, 0) };
        root.AddThemeConstantOverride("separation", 8);
        AddChild(root);

        var grid = new GridContainer { Columns = 2 };
        grid.AddThemeConstantOverride("h_separation", 12);
        grid.AddThemeConstantOverride("v_separation", 8);
        grid.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        root.AddChild(grid);

        _scriptNote = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(ContentWidth, 0),
            Visible = false
        };
        _scriptNote.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        root.AddChild(_scriptNote);

        _sceneField = AddRow<LineEdit>(grid, "Scene:", out _);
        _sceneField.Editable = false;

        _outputNameField = AddRow<LineEdit>(grid, "Output Name:", out _outputNameLabel);
        _outputNameField.PlaceholderText = "e.g. Player";
        _outputNameField.TooltipText = "The scene's root has no C# script, so its ids go into a class with this name.\n" +
                                       "Must be a valid C# class name.";
        _outputNameField.TextChanged += _ => UpdateOkButton();

        _flattenField = new CheckBox
        {
            Text = "Flatten (merge into one Animations class)",
            TooltipText = "On: all animations of all nodes in one class: Player.Animations.Idle.\n" +
                          "Off: one class per node: Player.AnimationPlayer.Idle."
        };
        root.AddChild(_flattenField);

        GetOkButton().Text = "Track";
        Confirmed += OnConfirmed;
    }

    private static T AddRow<T>(GridContainer grid, string labelText, out Label label) where T : Control, new()
    {
        label = new Label { Text = labelText };
        grid.AddChild(label);

        var field = new T { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        grid.AddChild(field);
        return field;
    }

    private void UpdateOkButton()
    {
        GetOkButton().Disabled = _nameRequired && !AnvilNaming.IsValidOutputName(_outputNameField.Text.Trim());
    }

    public void OpenFor(string scenePath)
    {
        _scenePath = scenePath;
        _existingRule = AnvilRuleManager.FindRuleForScene(scenePath);
        _sceneField.Text = scenePath;

        var info = AnvilSceneReader.Read(scenePath);
        _nameRequired = info is null || info.ScriptPath.Length == 0;

        _outputNameLabel.Visible = _nameRequired;
        _outputNameField.Visible = _nameRequired;

        _scriptNote.Visible = !_nameRequired;

        if (!_nameRequired)
        {
            _scriptNote.Text = $"This scene's root has a script ({info.ScriptPath.GetFile()}), so the ids are " +
                            "added to that script's class. A custom output name doesn't apply.";
        }

        if (_existingRule is not null)
        {
            _outputNameField.Text = _existingRule.OutputName;
            _flattenField.ButtonPressed = _existingRule.Flatten;
        }
        else
        {
            _outputNameField.Text = AnvilNaming.ToIdentifier(scenePath.GetFile().GetBaseName()) ?? "";
            _flattenField.ButtonPressed = true;
        }

        UpdateOkButton();

        PopupCentered(new Vector2I(ContentWidth + 32, 0));

        if (_nameRequired)
        {
            _outputNameField.GrabFocus();
            _outputNameField.SelectAll();
        }
    }

    private void OnConfirmed()
    {
        string outputName = _outputNameField.Text.Trim();

        if (_nameRequired)
        {
            if (!AnvilNaming.IsValidOutputName(outputName))
            {
                GD.PushWarning($"Anvil: Cannot track '{_scenePath}' with the output name '{outputName}'.");
                return;
            }
        }
        else
        {
            outputName = _existingRule?.OutputName is { Length: > 0 } existing
                ? existing
                : AnvilNaming.ToIdentifier(_scenePath.GetFile().GetBaseName()) ?? "";
        }

        var newRule = new AnvilSceneRule
        {
            ScenePath = _scenePath,
            SceneUid = AnvilSceneRule.GetUidText(_scenePath),
            OutputName = outputName,
            Sources = SceneSources.All,
            Flatten = _flattenField.ButtonPressed
        };

        AnvilRuleManager.SaveSceneRule(newRule, _existingRule);
    }
}
#endif