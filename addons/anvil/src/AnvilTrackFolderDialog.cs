#if TOOLS
using Godot;
using System;

namespace Anvil;

public partial class AnvilTrackFolderDialog : ConfirmationDialog
{
    private const int ContentWidth = 440;

    private LineEdit _sourceFolderField;
    private LineEdit _outputNameField;
    private CheckBox _recursiveField;
    private OptionButton _modeField;
    private LineEdit _extensionsField;

    private string _sourceFolder;
    private AnvilRule _existingRule;

    public override void _Ready()
    {
        Title = "Track Folder with Anvil";
        Unresizable = false;
        Exclusive = true;

        var root = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(ContentWidth, 0)
        };

        root.AddThemeConstantOverride("separation", 8);
        AddChild(root);

        var grid = new GridContainer { Columns = 2 };
        grid.AddThemeConstantOverride("h_separation", 12);
        grid.AddThemeConstantOverride("v_separation", 8);
        grid.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        root.AddChild(grid);

        _sourceFolderField = AddRow<LineEdit>(grid, "Source Folder:");
        _sourceFolderField.Editable = false;

        _outputNameField = AddRow<LineEdit>(grid, "Output Name:");
        _outputNameField.PlaceholderText = "e.g. Sfx";
        _outputNameField.TooltipText = "A valid C# class name: letters, digits and '_', not starting with a digit.";
        _outputNameField.TextChanged += _ => UpdateOkButton();

        _modeField = AddRow<OptionButton>(grid, "Mode:");
        _modeField.AddItem(nameof(ForgeMode.Id), (int)ForgeMode.Id);
        _modeField.AddItem(nameof(ForgeMode.FullPath), (int)ForgeMode.FullPath);

        _extensionsField = AddRow<LineEdit>(grid, "Extensions:");
        _extensionsField.PlaceholderText = "png, tscn, ogg";
        _extensionsField.ClearButtonEnabled = true;

        _recursiveField = new CheckBox { Text = "Recursive" };
        root.AddChild(_recursiveField);

        var extensionsHint = new Label
        {
            Text = "Extensions: comma separated, leave empty to include all files.",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(ContentWidth, 0)
        };
        extensionsHint.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        root.AddChild(extensionsHint);

        GetOkButton().Text = "Track";
        Confirmed += OnConfirmed;
    }

    private static T AddRow<T>(GridContainer grid, string labelText) where T : Control, new()
    {
        grid.AddChild(new Label { Text = labelText });

        var field = new T { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        grid.AddChild(field);
        return field;
    }

    private void UpdateOkButton()
    {
        GetOkButton().Disabled = !AnvilNaming.IsValidOutputName(_outputNameField.Text.Trim());
    }

    public void OpenFor(string sourceFolder)
    {
        _sourceFolder = sourceFolder;
        _existingRule = AnvilRuleManager.FindRuleForFolder(sourceFolder);
        _sourceFolderField.Text = sourceFolder;

        if (_existingRule is not null)
        {
            _outputNameField.Text = _existingRule.OutputName;
            _recursiveField.ButtonPressed = _existingRule.Recursive;
            _modeField.Select(_modeField.GetItemIndex((int)_existingRule.Mode));

            _extensionsField.Text = _existingRule.IncludedExtensions is { Count: > 0 } extensions
                ? string.Join(", ", extensions)
                : string.Empty;
        }
        else
        {
            _outputNameField.Text = sourceFolder.TrimEnd('/').GetFile().ToPascalCase();
            _recursiveField.ButtonPressed = false;
            _modeField.Select(_modeField.GetItemIndex((int)ForgeMode.Id));
            _extensionsField.Text = string.Empty;
        }

        UpdateOkButton();

        PopupCentered(new Vector2I(ContentWidth + 32, 0));
        _outputNameField.GrabFocus();
        _outputNameField.SelectAll();
    }

    private Godot.Collections.Array<string> GetIncludedExtensions()
    {
        Godot.Collections.Array<string> extensions = [];

        string[] parts = _extensionsField.Text.Split(
            [',', ';', ' ', '\t'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (string part in parts)
        {
            string extension = part.TrimStart('.', '*');
            if (extension.Length == 0)
                continue;

            extension = "." + extension;

            bool alreadyAdded = false;
            foreach (string existing in extensions)
            {
                if (string.Equals(existing, extension, StringComparison.OrdinalIgnoreCase))
                {
                    alreadyAdded = true;
                    break;
                }
            }

            if (!alreadyAdded)
                extensions.Add(extension);
        }

        return extensions;
    }

    private void OnConfirmed()
    {
        string outputName = _outputNameField.Text.Trim();

        if (string.IsNullOrWhiteSpace(outputName))
        {
            GD.PushWarning($"Anvil: Cannot track '{_sourceFolder}' with an empty output name.");
            return;
        }

        var newRule = new AnvilRule
        {
            SourceFolder = _sourceFolder,
            OutputName = outputName,
            Recursive = _recursiveField.ButtonPressed,
            Mode = (ForgeMode)_modeField.GetSelectedId(),
            IncludedExtensions = GetIncludedExtensions()
        };

        AnvilRuleManager.SaveRule(newRule, _existingRule);
    }
}
#endif