#if TOOLS
using Godot;

namespace Anvil;

/// <summary>
/// Dialog shown when tracking (or re-configuring) a folder from the FileSystem dock's
/// right-click context menu. Lets the user set the output class name, recursion, and mode
/// before the rule is saved via AnvilRuleManager.
/// </summary>
public partial class AnvilTrackFolderDialog : ConfirmationDialog
{
    private LineEdit _sourceFolderField;
    private LineEdit _outputNameField;
    private CheckBox _recursiveField;
    private OptionButton _modeField;

    private string _sourceFolder;
    private AnvilRule _existingRule;

    public override void _Ready()
    {
        Title = "Track Folder with Anvil";
        MinSize = new Vector2I(360, 0);

        var root = new VBoxContainer();
        root.AddThemeConstantOverride("separation", 8);
        AddChild(root);

        AddField(root, "Source Folder:", out _sourceFolderField);
        _sourceFolderField.Editable = false;

        AddField(root, "Output Name:", out _outputNameField);
        _outputNameField.PlaceholderText = "e.g. Sfx";

        var recursiveRow = new HBoxContainer();
        root.AddChild(recursiveRow);
        recursiveRow.AddChild(new Label { Text = "Recursive:" });
        _recursiveField = new CheckBox();
        recursiveRow.AddChild(_recursiveField);

        var modeRow = new HBoxContainer();
        root.AddChild(modeRow);
        modeRow.AddChild(new Label { Text = "Mode:" });
        _modeField = new OptionButton();
        _modeField.AddItem(nameof(ForgeMode.Id), (int)ForgeMode.Id);
        _modeField.AddItem(nameof(ForgeMode.FullPath), (int)ForgeMode.FullPath);
        modeRow.AddChild(_modeField);

        GetOkButton().Text = "Track";
        Confirmed += OnConfirmed;
    }

    private static void AddField(VBoxContainer root, string labelText, out LineEdit field)
    {
        root.AddChild(new Label { Text = labelText });
        field = new LineEdit();
        root.AddChild(field);
    }

    /// <summary>
    /// Opens the dialog for a folder. If an existing rule already tracks this folder,
    /// its values are pre-filled and saving will replace that rule instead of adding a new one.
    /// </summary>
    public void OpenFor(string sourceFolder)
    {
        _sourceFolder = sourceFolder;
        _existingRule = AnvilRuleManager.FindRuleForFolder(sourceFolder);

        _sourceFolderField.Text = sourceFolder;

        if (_existingRule is not null)
        {
            _outputNameField.Text = _existingRule.OutputName;
            _recursiveField.ButtonPressed = _existingRule.Recursive;
            _modeField.Selected = (int)_existingRule.Mode;
        }
        else
        {
            string suggestedName = sourceFolder.TrimEnd('/').GetFile().ToPascalCase();
            _outputNameField.Text = suggestedName;
            _recursiveField.ButtonPressed = false;
            _modeField.Selected = (int)ForgeMode.Id;
        }

        PopupCentered();
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
            Mode = (ForgeMode)_modeField.Selected,
        };

        AnvilRuleManager.SaveRule(newRule, _existingRule);
    }
}
#endif
