using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Edge.A11y
{
    /// <summary>
    /// A tool to convert element names into codes and back again
    /// </summary>
    public class ElementConverter
    {
        Dictionary<int, string> UI8Mapping = new Dictionary<int, string>();

        /// <summary>
        /// Convert a code into an element name
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetElementNameFromCode(int key)
        {
            if (UI8Mapping.ContainsKey(key))
            {
                return UI8Mapping[key];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Convert an element name into a code
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetElementCodeFromName(string name)
        {
            return UI8Mapping.First(n => n.Value.Equals(name, StringComparison.CurrentCultureIgnoreCase)).Key;
        }

        /// <summary>
        /// Create an element converter 
        /// </summary>
        public ElementConverter()
        {
            Initialize();
        }

        /// <summary>
        /// Do the work of creating an element converter for a given browser
        /// </summary>
        private void Initialize()
        {
            //PropertyIds
            UI8Mapping.Add(30000, "RuntimeId");
            UI8Mapping.Add(30001, "BoundingRectangle");
            UI8Mapping.Add(30002, "ProcessId");
            UI8Mapping.Add(30003, "ControlType");
            UI8Mapping.Add(30004, "LocalizedControlType");
            UI8Mapping.Add(30005, "Name");
            UI8Mapping.Add(30006, "AcceleratorKey");
            UI8Mapping.Add(30007, "AccessKey");
            UI8Mapping.Add(30008, "HasKeyboardFocus");
            UI8Mapping.Add(30009, "IsKeyboardFocusable");
            UI8Mapping.Add(30010, "IsEnabled");
            UI8Mapping.Add(30011, "AutomationId");
            UI8Mapping.Add(30012, "ClassName");
            UI8Mapping.Add(30013, "HelpText");
            UI8Mapping.Add(30014, "ClickablePoint");
            UI8Mapping.Add(30015, "Culture");
            UI8Mapping.Add(30016, "IsControlElement");
            UI8Mapping.Add(30017, "IsContentElement");
            UI8Mapping.Add(30018, "LabeledBy");
            UI8Mapping.Add(30019, "IsPassword");
            UI8Mapping.Add(30020, "NativeWindowHandle");
            UI8Mapping.Add(30021, "ItemType");
            UI8Mapping.Add(30022, "IsOffscreen");
            UI8Mapping.Add(30023, "Orientation");
            UI8Mapping.Add(30024, "FrameworkId");
            UI8Mapping.Add(30025, "IsRequiredForForm");
            UI8Mapping.Add(30026, "ItemStatus");
            UI8Mapping.Add(30027, "IsDockPatternAvailable");
            UI8Mapping.Add(30028, "IsExpandCollapsePatternAvailable");
            UI8Mapping.Add(30029, "IsGridItemPatternAvailable");
            UI8Mapping.Add(30030, "IsGridPatternAvailable");
            UI8Mapping.Add(30031, "IsInvokePatternAvailable");
            UI8Mapping.Add(30032, "IsMultipleViewPatternAvailable");
            UI8Mapping.Add(30033, "IsRangeValuePatternAvailable");
            UI8Mapping.Add(30034, "IsScrollPatternAvailable");
            UI8Mapping.Add(30035, "IsScrollItemPatternAvailable");
            UI8Mapping.Add(30036, "IsSelectionItemPatternAvailable");
            UI8Mapping.Add(30037, "IsSelectionPatternAvailable");
            UI8Mapping.Add(30038, "IsTablePatternAvailable");
            UI8Mapping.Add(30039, "IsTableItemPatternAvailable");
            UI8Mapping.Add(30040, "IsTextPatternAvailable");
            UI8Mapping.Add(30041, "IsTogglePatternAvailable");
            UI8Mapping.Add(30042, "IsTransformPatternAvailable");
            UI8Mapping.Add(30043, "IsValuePatternAvailable");
            UI8Mapping.Add(30044, "IsWindowPatternAvailable");
            UI8Mapping.Add(30045, "ValueValue");
            UI8Mapping.Add(30046, "ValueIsReadOnly");
            UI8Mapping.Add(30047, "RangeValueValue");
            UI8Mapping.Add(30048, "RangeValueIsReadOnly");
            UI8Mapping.Add(30049, "RangeValueMinimum");
            UI8Mapping.Add(30050, "RangeValueMaximum");
            UI8Mapping.Add(30051, "RangeValueLargeChange");
            UI8Mapping.Add(30052, "RangeValueSmallChange");
            UI8Mapping.Add(30053, "ScrollHorizontalScrollPercent");
            UI8Mapping.Add(30054, "ScrollHorizontalViewSize");
            UI8Mapping.Add(30055, "ScrollVerticalScrollPercent");
            UI8Mapping.Add(30056, "ScrollVerticalViewSize");
            UI8Mapping.Add(30057, "ScrollHorizontallyScrollable");
            UI8Mapping.Add(30058, "ScrollVerticallyScrollable");
            UI8Mapping.Add(30059, "SelectionSelection");
            UI8Mapping.Add(30060, "SelectionCanSelectMultiple");
            UI8Mapping.Add(30061, "SelectionIsSelectionRequired");
            UI8Mapping.Add(30062, "GridRowCount");
            UI8Mapping.Add(30063, "GridColumnCount");
            UI8Mapping.Add(30064, "GridItemRow");
            UI8Mapping.Add(30065, "GridItemColumn");
            UI8Mapping.Add(30066, "GridItemRowSpan");
            UI8Mapping.Add(30067, "GridItemColumnSpan");
            UI8Mapping.Add(30068, "GridItemContainingGrid");
            UI8Mapping.Add(30069, "DockDockPosition");
            UI8Mapping.Add(30070, "ExpandCollapseExpandCollapseState");
            UI8Mapping.Add(30071, "MultipleViewCurrentView");
            UI8Mapping.Add(30072, "MultipleViewSupportedViews");
            UI8Mapping.Add(30073, "WindowCanMaximize");
            UI8Mapping.Add(30074, "WindowCanMinimize");
            UI8Mapping.Add(30075, "WindowWindowVisualState");
            UI8Mapping.Add(30076, "WindowWindowInteractionState");
            UI8Mapping.Add(30077, "WindowIsModal");
            UI8Mapping.Add(30078, "WindowIsTopmost");
            UI8Mapping.Add(30079, "SelectionItemIsSelected");
            UI8Mapping.Add(30080, "SelectionItemSelectionContainer");
            UI8Mapping.Add(30081, "TableRowHeaders");
            UI8Mapping.Add(30082, "TableColumnHeaders");
            UI8Mapping.Add(30083, "TableRowOrColumnMajor");
            UI8Mapping.Add(30084, "TableItemRowHeaderItems");
            UI8Mapping.Add(30085, "TableItemColumnHeaderItems");
            UI8Mapping.Add(30086, "ToggleToggleState");
            UI8Mapping.Add(30087, "TransformCanMove");
            UI8Mapping.Add(30088, "TransformCanResize");
            UI8Mapping.Add(30089, "TransformCanRotate");
            UI8Mapping.Add(30090, "IsLegacyIAccessiblePatternAvailable");
            UI8Mapping.Add(30091, "LegacyIAccessibleChildId");
            UI8Mapping.Add(30092, "LegacyIAccessibleName");
            UI8Mapping.Add(30093, "LegacyIAccessibleValue");
            UI8Mapping.Add(30094, "LegacyIAccessibleDescription");
            UI8Mapping.Add(30095, "LegacyIAccessibleRole");
            UI8Mapping.Add(30096, "LegacyIAccessibleState");
            UI8Mapping.Add(30097, "LegacyIAccessibleHelp");
            UI8Mapping.Add(30098, "LegacyIAccessibleKeyboardShortcut");
            UI8Mapping.Add(30099, "LegacyIAccessibleSelection");
            UI8Mapping.Add(30100, "LegacyIAccessibleDefaultAction");
            UI8Mapping.Add(30101, "AriaRole");
            UI8Mapping.Add(30102, "AriaProperties");
            UI8Mapping.Add(30103, "IsDataValidForForm");
            UI8Mapping.Add(30104, "ControllerFor");
            UI8Mapping.Add(30105, "DescribedBy");
            UI8Mapping.Add(30106, "FlowsTo");
            UI8Mapping.Add(30107, "ProviderDescription");
            UI8Mapping.Add(30108, "IsItemContainerPatternAvailable");
            UI8Mapping.Add(30109, "IsVirtualizedItemPatternAvailable");
            UI8Mapping.Add(30110, "IsSynchronizedInputPatternAvailable");

            //TODO use UIA_ControlTypeIds
            //AttributeIds
            UI8Mapping.Add(40000, "AnimationStyle");
            UI8Mapping.Add(40001, "BackgroundColor");
            UI8Mapping.Add(40002, "BulletStyle");
            UI8Mapping.Add(40003, "CapStyle");
            UI8Mapping.Add(40004, "Culture");
            UI8Mapping.Add(40005, "FontName");
            UI8Mapping.Add(40006, "FontSize");
            UI8Mapping.Add(40007, "FontWeight");
            UI8Mapping.Add(40008, "ForegroundColor");
            UI8Mapping.Add(40009, "HorizontalTextAlignment");
            UI8Mapping.Add(40010, "IndentationFirstLine");
            UI8Mapping.Add(40011, "IndentationLeading");
            UI8Mapping.Add(40012, "IndentationTrailing");
            UI8Mapping.Add(40013, "IsHidden");
            UI8Mapping.Add(40014, "IsItalic");
            UI8Mapping.Add(40015, "IsReadOnly");
            UI8Mapping.Add(40016, "IsSubscript");
            UI8Mapping.Add(40017, "IsSuperscript");
            UI8Mapping.Add(40018, "MarginBottom");
            UI8Mapping.Add(40019, "MarginLeading");
            UI8Mapping.Add(40020, "MarginTop");
            UI8Mapping.Add(40021, "MarginTrailing");
            UI8Mapping.Add(40022, "OutlineStyles");
            UI8Mapping.Add(40023, "OverlineColor");
            UI8Mapping.Add(40024, "OverlineStyle");
            UI8Mapping.Add(40025, "StrikethroughColor");
            UI8Mapping.Add(40026, "StrikethroughStyle");
            UI8Mapping.Add(40027, "Tabs");
            UI8Mapping.Add(40028, "TextFlowDirections");
            UI8Mapping.Add(40029, "UnderlineColor");
            UI8Mapping.Add(40030, "UnderlineStyle");
            UI8Mapping.Add(40031, "AnnotationTypes");
            UI8Mapping.Add(40032, "AnnotationObjects");
            UI8Mapping.Add(40033, "StyleName");
            UI8Mapping.Add(40034, "StyleId");
            UI8Mapping.Add(40035, "Link");
            UI8Mapping.Add(40036, "IsActive");
            UI8Mapping.Add(40037, "SelectionActiveEnd");
            UI8Mapping.Add(40038, "CaretPosition");
            UI8Mapping.Add(40039, "CaretBidiMode");

            //Control types
            UI8Mapping.Add(50000, "Button");
            UI8Mapping.Add(50001, "Calendar");
            UI8Mapping.Add(50002, "Checkbox");
            UI8Mapping.Add(50003, "Combobox");
            UI8Mapping.Add(50004, "Edit");
            UI8Mapping.Add(50005, "Hyperlink");
            UI8Mapping.Add(50006, "Image");
            UI8Mapping.Add(50007, "Listitem");
            UI8Mapping.Add(50008, "List");
            UI8Mapping.Add(50009, "Menu");
            UI8Mapping.Add(50010, "Menubar");
            UI8Mapping.Add(50011, "Menuitem");
            UI8Mapping.Add(50012, "Progressbar");
            UI8Mapping.Add(50013, "Radiobutton");
            UI8Mapping.Add(50014, "Scrollbar");
            UI8Mapping.Add(50015, "Slider");
            UI8Mapping.Add(50016, "Spinner");
            UI8Mapping.Add(50017, "Statusbar");
            UI8Mapping.Add(50018, "Tab");
            UI8Mapping.Add(50019, "Tabitem");
            UI8Mapping.Add(50020, "Text");
            UI8Mapping.Add(50021, "Toolbar");
            UI8Mapping.Add(50022, "Tooltip");
            UI8Mapping.Add(50023, "Tree");
            UI8Mapping.Add(50024, "Treeitem");
            UI8Mapping.Add(50025, "Custom");
            UI8Mapping.Add(50026, "Group");
            UI8Mapping.Add(50027, "Thumb");
            UI8Mapping.Add(50028, "Datagrid");
            UI8Mapping.Add(50029, "Dataitem");
            UI8Mapping.Add(50030, "Document");
            UI8Mapping.Add(50031, "Splitbutton");
            UI8Mapping.Add(50032, "Window");
            UI8Mapping.Add(50033, "Pane");
            UI8Mapping.Add(50034, "Header");
            UI8Mapping.Add(50035, "Headeritem");
            UI8Mapping.Add(50036, "Table");
            UI8Mapping.Add(50037, "Titlebar");
            UI8Mapping.Add(50038, "Separator");
            UI8Mapping.Add(50039, "Semanticzoom");
            UI8Mapping.Add(50040, "Appbar");

            UI8Mapping.Add(80000, "Custom");
            UI8Mapping.Add(80001, "Form");
            UI8Mapping.Add(80002, "Main");
            UI8Mapping.Add(80003, "Navigation");
            UI8Mapping.Add(80004, "Search");
        }
    }
}
