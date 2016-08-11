namespace Microsoft.Edge.A11y
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A tool to convert element names into codes and back again
    /// </summary>
    public static class ElementConverter
    {
        /// <summary>
        /// The stored control type mapping
        /// </summary>
        private static Dictionary<int, UIAControlType> controlTypeMapping;

        /// <summary>
        /// The stored property mapping
        /// </summary>
        private static Dictionary<int, UIAProperty> propertyMapping;

        /// <summary>
        /// The stored landmark mapping
        /// </summary>
        private static Dictionary<int, UIALandmarkType> landmarkTypeMapping;

        /// <summary>
        /// The stored key mapping
        /// </summary>
        private static Dictionary<WebDriverKey, string> webDriverKeyMapping;

        /// <summary>
        /// The different control types that elements in UIA can have, as well as unknown
        /// </summary>
        public enum UIAControlType
        {
            /// <summary>
            /// Unknown control type
            /// </summary>
            Unknown,

            /// <summary>
            /// Button control type
            /// </summary>
            Button,

            /// <summary>
            /// Calendar control type
            /// </summary>
            Calendar,

            /// <summary>
            /// Checkbox control type
            /// </summary>
            Checkbox,

            /// <summary>
            /// Combo box control type
            /// </summary>
            Combobox,

            /// <summary>
            /// Edit control type
            /// </summary>
            Edit,

            /// <summary>
            /// Hyperlink control type
            /// </summary>
            Hyperlink,

            /// <summary>
            /// Image control type
            /// </summary>
            Image,

            /// <summary>
            /// List item control type
            /// </summary>
            Listitem,

            /// <summary>
            /// List control type
            /// </summary>
            List,

            /// <summary>
            /// Menu control type
            /// </summary>
            Menu,

            /// <summary>
            /// Menu bar control type
            /// </summary>
            Menubar,

            /// <summary>
            /// Menu item control type
            /// </summary>
            Menuitem,

            /// <summary>
            /// Progress bar control type
            /// </summary>
            Progressbar,

            /// <summary>
            /// Radio button control type
            /// </summary>
            Radiobutton,

            /// <summary>
            /// Scrollbar control type
            /// </summary>
            Scrollbar,

            /// <summary>
            /// Slider control type
            /// </summary>
            Slider,

            /// <summary>
            /// Spinner control type
            /// </summary>
            Spinner,

            /// <summary>
            /// Status bar control type
            /// </summary>
            Statusbar,

            /// <summary>
            /// Tab control type
            /// </summary>
            Tab,

            /// <summary>
            /// Tab item control type
            /// </summary>
            Tabitem,

            /// <summary>
            /// Text control type
            /// </summary>
            Text,

            /// <summary>
            /// Toolbar control type
            /// </summary>
            Toolbar,

            /// <summary>
            /// Tooltip control type
            /// </summary>
            Tooltip,

            /// <summary>
            /// Tree control type
            /// </summary>
            Tree,

            /// <summary>
            /// Tree item control type
            /// </summary>
            Treeitem,

            /// <summary>
            /// Custom control type
            /// </summary>
            Custom,

            /// <summary>
            /// Group control type
            /// </summary>
            Group,

            /// <summary>
            /// Thumb control type
            /// </summary>
            Thumb,

            /// <summary>
            /// Data grid control type
            /// </summary>
            Datagrid,

            /// <summary>
            /// Data item control type
            /// </summary>
            Dataitem,

            /// <summary>
            /// Document control type
            /// </summary>
            Document,

            /// <summary>
            /// Split button control type
            /// </summary>
            Splitbutton,

            /// <summary>
            /// Window control type
            /// </summary>
            Window,

            /// <summary>
            /// Pane control type
            /// </summary>
            Pane,

            /// <summary>
            /// Header control type
            /// </summary>
            Header,

            /// <summary>
            /// Header item control type
            /// </summary>
            Headeritem,

            /// <summary>
            /// Table control type
            /// </summary>
            Table,

            /// <summary>
            /// Title bar control type
            /// </summary>
            Titlebar,

            /// <summary>
            /// Separator control type
            /// </summary>
            Separator,

            /// <summary>
            /// Semantic zoom control type
            /// </summary>
            Semanticzoom,

            /// <summary>
            /// App bar control type
            /// </summary>
            Appbar,
        }

        /// <summary>
        /// The different property names that exist in UIA, as well as unknown
        /// </summary>
        public enum UIAProperty
        {
            /// <summary>
            /// Unknown property
            /// </summary>
            Unknown,

            /// <summary>
            /// RuntimeId property
            /// </summary>
            RuntimeId,

            /// <summary>
            /// BoundingRectangle property
            /// </summary>
            BoundingRectangle,

            /// <summary>
            /// ProcessId property
            /// </summary>
            ProcessId,

            /// <summary>
            /// ControlType property
            /// </summary>
            ControlType,

            /// <summary>
            /// LocalizedControlType property
            /// </summary>
            LocalizedControlType,

            /// <summary>
            /// Name property
            /// </summary>
            Name,

            /// <summary>
            /// AcceleratorKey property
            /// </summary>
            AcceleratorKey,

            /// <summary>
            /// AccessKey property
            /// </summary>
            AccessKey,

            /// <summary>
            /// HasKeyboardFocus property
            /// </summary>
            HasKeyboardFocus,

            /// <summary>
            /// IsKeyboardFocusable property
            /// </summary>
            IsKeyboardFocusable,

            /// <summary>
            /// IsEnabled property
            /// </summary>
            IsEnabled,

            /// <summary>
            /// AutomationId property
            /// </summary>
            AutomationId,

            /// <summary>
            /// ClassName property
            /// </summary>
            ClassName,

            /// <summary>
            /// HelpText property
            /// </summary>
            HelpText,

            /// <summary>
            /// ClickablePoint property
            /// </summary>
            ClickablePoint,

            /// <summary>
            /// Culture property
            /// </summary>
            Culture,

            /// <summary>
            /// IsControlElement property
            /// </summary>
            IsControlElement,

            /// <summary>
            /// IsContentElement property
            /// </summary>
            IsContentElement,

            /// <summary>
            /// LabeledBy property
            /// </summary>
            LabeledBy,

            /// <summary>
            /// IsPassword property
            /// </summary>
            IsPassword,

            /// <summary>
            /// NativeWindowHandle property
            /// </summary>
            NativeWindowHandle,

            /// <summary>
            /// ItemType property
            /// </summary>
            ItemType,

            /// <summary>
            /// Is Off screen property
            /// </summary>
            IsOffscreen,

            /// <summary>
            /// Orientation property
            /// </summary>
            Orientation,

            /// <summary>
            /// FrameworkId property
            /// </summary>
            FrameworkId,

            /// <summary>
            /// IsRequiredForForm property
            /// </summary>
            IsRequiredForForm,

            /// <summary>
            /// ItemStatus property
            /// </summary>
            ItemStatus,

            /// <summary>
            /// IsDockPatternAvailable property
            /// </summary>
            IsDockPatternAvailable,

            /// <summary>
            /// IsExpandCollapsePatternAvailable property
            /// </summary>
            IsExpandCollapsePatternAvailable,

            /// <summary>
            /// IsGridItemPatternAvailable property
            /// </summary>
            IsGridItemPatternAvailable,

            /// <summary>
            /// IsGridPatternAvailable property
            /// </summary>
            IsGridPatternAvailable,

            /// <summary>
            /// IsInvokePatternAvailable property
            /// </summary>
            IsInvokePatternAvailable,

            /// <summary>
            /// IsMultipleViewPatternAvailable property
            /// </summary>
            IsMultipleViewPatternAvailable,

            /// <summary>
            /// IsRangeValuePatternAvailable property
            /// </summary>
            IsRangeValuePatternAvailable,

            /// <summary>
            /// IsScrollPatternAvailable property
            /// </summary>
            IsScrollPatternAvailable,

            /// <summary>
            /// IsScrollItemPatternAvailable property
            /// </summary>
            IsScrollItemPatternAvailable,

            /// <summary>
            /// IsSelectionItemPatternAvailable property
            /// </summary>
            IsSelectionItemPatternAvailable,

            /// <summary>
            /// IsSelectionPatternAvailable property
            /// </summary>
            IsSelectionPatternAvailable,

            /// <summary>
            /// IsTablePatternAvailable property
            /// </summary>
            IsTablePatternAvailable,

            /// <summary>
            /// IsTableItemPatternAvailable property
            /// </summary>
            IsTableItemPatternAvailable,

            /// <summary>
            /// IsTextPatternAvailable property
            /// </summary>
            IsTextPatternAvailable,

            /// <summary>
            /// IsTogglePatternAvailable property
            /// </summary>
            IsTogglePatternAvailable,

            /// <summary>
            /// IsTransformPatternAvailable property
            /// </summary>
            IsTransformPatternAvailable,

            /// <summary>
            /// IsValuePatternAvailable property
            /// </summary>
            IsValuePatternAvailable,

            /// <summary>
            /// IsWindowPatternAvailable property
            /// </summary>
            IsWindowPatternAvailable,

            /// <summary>
            /// ValueValue property
            /// </summary>
            ValueValue,

            /// <summary>
            /// ValueIsReadOnly property
            /// </summary>
            ValueIsReadOnly,

            /// <summary>
            /// RangeValueValue property
            /// </summary>
            RangeValueValue,

            /// <summary>
            /// RangeValueIsReadOnly property
            /// </summary>
            RangeValueIsReadOnly,

            /// <summary>
            /// RangeValueMinimum property
            /// </summary>
            RangeValueMinimum,

            /// <summary>
            /// RangeValueMaximum property
            /// </summary>
            RangeValueMaximum,

            /// <summary>
            /// RangeValueLargeChange property
            /// </summary>
            RangeValueLargeChange,

            /// <summary>
            /// RangeValueSmallChange property
            /// </summary>
            RangeValueSmallChange,

            /// <summary>
            /// ScrollHorizontalScrollPercent property
            /// </summary>
            ScrollHorizontalScrollPercent,

            /// <summary>
            /// ScrollHorizontalViewSize property
            /// </summary>
            ScrollHorizontalViewSize,

            /// <summary>
            /// ScrollVerticalScrollPercent property
            /// </summary>
            ScrollVerticalScrollPercent,

            /// <summary>
            /// ScrollVerticalViewSize property
            /// </summary>
            ScrollVerticalViewSize,

            /// <summary>
            /// ScrollHorizontallyScrollable property
            /// </summary>
            ScrollHorizontallyScrollable,

            /// <summary>
            /// ScrollVerticallyScrollable property
            /// </summary>
            ScrollVerticallyScrollable,

            /// <summary>
            /// SelectionSelection property
            /// </summary>
            SelectionSelection,

            /// <summary>
            /// SelectionCanSelectMultiple property
            /// </summary>
            SelectionCanSelectMultiple,

            /// <summary>
            /// SelectionIsSelectionRequired property
            /// </summary>
            SelectionIsSelectionRequired,

            /// <summary>
            /// GridRowCount property
            /// </summary>
            GridRowCount,

            /// <summary>
            /// GridColumnCount property
            /// </summary>
            GridColumnCount,

            /// <summary>
            /// GridItemRow property
            /// </summary>
            GridItemRow,

            /// <summary>
            /// GridItemColumn property
            /// </summary>
            GridItemColumn,

            /// <summary>
            /// GridItemRowSpan property
            /// </summary>
            GridItemRowSpan,

            /// <summary>
            /// GridItemColumnSpan property
            /// </summary>
            GridItemColumnSpan,

            /// <summary>
            /// GridItemContainingGrid property
            /// </summary>
            GridItemContainingGrid,

            /// <summary>
            /// DockDockPosition property
            /// </summary>
            DockDockPosition,

            /// <summary>
            /// ExpandCollapseExpandCollapseState property
            /// </summary>
            ExpandCollapseExpandCollapseState,

            /// <summary>
            /// MultipleViewCurrentView property
            /// </summary>
            MultipleViewCurrentView,

            /// <summary>
            /// MultipleViewSupportedViews property
            /// </summary>
            MultipleViewSupportedViews,

            /// <summary>
            /// WindowCanMaximize property
            /// </summary>
            WindowCanMaximize,

            /// <summary>
            /// WindowCanMinimize property
            /// </summary>
            WindowCanMinimize,

            /// <summary>
            /// WindowWindowVisualState property
            /// </summary>
            WindowWindowVisualState,

            /// <summary>
            /// WindowWindowInteractionState property
            /// </summary>
            WindowWindowInteractionState,

            /// <summary>
            /// WindowIsModal property
            /// </summary>
            WindowIsModal,

            /// <summary>
            /// WindowIsTopmost property
            /// </summary>
            WindowIsTopmost,

            /// <summary>
            /// SelectionItemIsSelected property
            /// </summary>
            SelectionItemIsSelected,

            /// <summary>
            /// SelectionItemSelectionContainer property
            /// </summary>
            SelectionItemSelectionContainer,

            /// <summary>
            /// TableRowHeaders property
            /// </summary>
            TableRowHeaders,

            /// <summary>
            /// TableColumnHeaders property
            /// </summary>
            TableColumnHeaders,

            /// <summary>
            /// TableRowOrColumnMajor property
            /// </summary>
            TableRowOrColumnMajor,

            /// <summary>
            /// TableItemRowHeaderItems property
            /// </summary>
            TableItemRowHeaderItems,

            /// <summary>
            /// TableItemColumnHeaderItems property
            /// </summary>
            TableItemColumnHeaderItems,

            /// <summary>
            /// ToggleToggleState property
            /// </summary>
            ToggleToggleState,

            /// <summary>
            /// TransformCanMove property
            /// </summary>
            TransformCanMove,

            /// <summary>
            /// TransformCanResize property
            /// </summary>
            TransformCanResize,

            /// <summary>
            /// TransformCanRotate property
            /// </summary>
            TransformCanRotate,

            /// <summary>
            /// IsLegacyIAccessiblePatternAvailable property
            /// </summary>
            IsLegacyIAccessiblePatternAvailable,

            /// <summary>
            /// LegacyIAccessibleChildId property
            /// </summary>
            LegacyIAccessibleChildId,

            /// <summary>
            /// LegacyIAccessibleName property
            /// </summary>
            LegacyIAccessibleName,

            /// <summary>
            /// LegacyIAccessibleValue property
            /// </summary>
            LegacyIAccessibleValue,

            /// <summary>
            /// LegacyIAccessibleDescription property
            /// </summary>
            LegacyIAccessibleDescription,

            /// <summary>
            /// LegacyIAccessibleRole property
            /// </summary>
            LegacyIAccessibleRole,

            /// <summary>
            /// LegacyIAccessibleState property
            /// </summary>
            LegacyIAccessibleState,

            /// <summary>
            /// LegacyIAccessibleHelp property
            /// </summary>
            LegacyIAccessibleHelp,

            /// <summary>
            /// LegacyIAccessibleKeyboardShortcut property
            /// </summary>
            LegacyIAccessibleKeyboardShortcut,

            /// <summary>
            /// LegacyIAccessibleSelection property
            /// </summary>
            LegacyIAccessibleSelection,

            /// <summary>
            /// LegacyIAccessibleDefaultAction property
            /// </summary>
            LegacyIAccessibleDefaultAction,

            /// <summary>
            /// AriaRole property
            /// </summary>
            AriaRole,

            /// <summary>
            /// AriaProperties property
            /// </summary>
            AriaProperties,

            /// <summary>
            /// IsDataValidForForm property
            /// </summary>
            IsDataValidForForm,

            /// <summary>
            /// ControllerFor property
            /// </summary>
            ControllerFor,

            /// <summary>
            /// DescribedBy property
            /// </summary>
            DescribedBy,

            /// <summary>
            /// FlowsTo property
            /// </summary>
            FlowsTo,

            /// <summary>
            /// ProviderDescription property
            /// </summary>
            ProviderDescription,

            /// <summary>
            /// IsItemContainerPatternAvailable property
            /// </summary>
            IsItemContainerPatternAvailable,

            /// <summary>
            /// IsVirtualizedItemPatternAvailable property
            /// </summary>
            IsVirtualizedItemPatternAvailable,

            /// <summary>
            /// IsSynchronizedInputPatternAvailable property
            /// </summary>
            IsSynchronizedInputPatternAvailable,
        }

        /// <summary>
        /// The different landmark types in UIA
        /// </summary>
        public enum UIALandmarkType
        {
            /// <summary>
            /// The Unknown landmark
            /// </summary>
            Unknown,

            /// <summary>
            /// The Custom landmark
            /// </summary>
            Custom,

            /// <summary>
            /// The Form landmark
            /// </summary>
            Form,

            /// <summary>
            /// The Main landmark
            /// </summary>
            Main,

            /// <summary>
            /// The Navigation landmark
            /// </summary>
            Navigation,

            /// <summary>
            /// The Search landmark
            /// </summary>
            Search
        }

        /// <summary>
        /// The different special keys that can be sent via WebDriver
        /// </summary>
        public enum WebDriverKey
        {
            /// <summary>
            /// The Null key
            /// </summary>
            Null,

            /// <summary>
            /// The Cancel key
            /// </summary>
            Cancel,

            /// <summary>
            /// The Help key
            /// </summary>
            Help,

            /// <summary>
            /// The Backspace key
            /// </summary>
            Back_space,

            /// <summary>
            /// The Tab key
            /// </summary>
            Tab,

            /// <summary>
            /// The Clear key
            /// </summary>
            Clear,

            /// <summary>
            /// The Return key
            /// </summary>
            Return,

            /// <summary>
            /// The Enter key
            /// </summary>
            Enter,

            /// <summary>
            /// The Shift key
            /// </summary>
            Shift,

            /// <summary>
            /// The Control key
            /// </summary>
            Control,

            /// <summary>
            /// The Alt key
            /// </summary>
            Alt,

            /// <summary>
            /// The Pause key
            /// </summary>
            Pause,

            /// <summary>
            /// The Escape key
            /// </summary>
            Escape,

            /// <summary>
            /// The Space key
            /// </summary>
            Space,

            /// <summary>
            /// The Page up key
            /// </summary>
            Page_up,

            /// <summary>
            /// The Page down key
            /// </summary>
            Page_down,

            /// <summary>
            /// The End key
            /// </summary>
            End,

            /// <summary>
            /// The Home key
            /// </summary>
            Home,

            /// <summary>
            /// The Arrow left key
            /// </summary>
            Arrow_left,

            /// <summary>
            /// The Arrow up key
            /// </summary>
            Arrow_up,

            /// <summary>
            /// The Arrow right key
            /// </summary>
            Arrow_right,

            /// <summary>
            /// The Arrow down key
            /// </summary>
            Arrow_down,

            /// <summary>
            /// The Insert key
            /// </summary>
            Insert,

            /// <summary>
            /// The Delete key
            /// </summary>
            Delete,

            /// <summary>
            /// The Semicolon key
            /// </summary>
            Semicolon,

            /// <summary>
            /// The Equals key
            /// </summary>
            Equals,

            /// <summary>
            /// The Numpad0 key
            /// </summary>
            Numpad0,

            /// <summary>
            /// The Numpad1 key
            /// </summary>
            Numpad1,

            /// <summary>
            /// The Numpad2 key
            /// </summary>
            Numpad2,

            /// <summary>
            /// The Numpad3 key
            /// </summary>
            Numpad3,

            /// <summary>
            /// The Numpad4 key
            /// </summary>
            Numpad4,

            /// <summary>
            /// The Numpad5 key
            /// </summary>
            Numpad5,

            /// <summary>
            /// The Numpad6 key
            /// </summary>
            Numpad6,

            /// <summary>
            /// The Numpad7 key
            /// </summary>
            Numpad7,

            /// <summary>
            /// The Numpad8 key
            /// </summary>
            Numpad8,

            /// <summary>
            /// The Numpad9 key
            /// </summary>
            Numpad9,

            /// <summary>
            /// The Multiply key
            /// </summary>
            Multiply,

            /// <summary>
            /// The Add key
            /// </summary>
            Add,

            /// <summary>
            /// The Separator key
            /// </summary>
            Separator,

            /// <summary>
            /// The Subtract key
            /// </summary>
            Subtract,

            /// <summary>
            /// The Decimal key
            /// </summary>
            Decimal,

            /// <summary>
            /// The Divide key
            /// </summary>
            Divide,

            /// <summary>
            /// The F1 key
            /// </summary>
            F1,

            /// <summary>
            /// The F2 key
            /// </summary>
            F2,

            /// <summary>
            /// The F3 key
            /// </summary>
            F3,

            /// <summary>
            /// The F4 key
            /// </summary>
            F4,

            /// <summary>
            /// The F5 key
            /// </summary>
            F5,

            /// <summary>
            /// The F6 key
            /// </summary>
            F6,

            /// <summary>
            /// The F7 key
            /// </summary>
            F7,

            /// <summary>
            /// The F8 key
            /// </summary>
            F8,

            /// <summary>
            /// The F9 key
            /// </summary>
            F9,

            /// <summary>
            /// The F10 key
            /// </summary>
            F10,

            /// <summary>
            /// The F11 key
            /// </summary>
            F11,

            /// <summary>
            /// The F12 key
            /// </summary>
            F12,

            /// <summary>
            /// The Meta key
            /// </summary>
            Meta,

            /// <summary>
            /// The Command key
            /// </summary>
            Command,

            /// <summary>
            /// The Zenkaku/hankaku key
            /// </summary>
            Zenkaku_hankaku,

            /// <summary>
            /// The Wait special character
            /// </summary>
            Wait
        }

        /// <summary>
        /// Convert a code into an element name
        /// </summary>
        /// <param name="code">The code to search for</param>
        /// <returns>The ControlType enum of the element, or Unknown if it is not found</returns>
        public static UIAControlType GetControlTypeFromCode(int code)
        {
            if (controlTypeMapping == null)
            {
                InitControlTypeMapping();
            }

            UIAControlType controlType;
            return controlTypeMapping.TryGetValue(code, out controlType) ? controlType : UIAControlType.Unknown;
        }

        /// <summary>
        /// Convert a code into a property
        /// </summary>
        /// <param name="code">The code to search for</param>
        /// <returns>The Property enum of the element, or Unknown if it is not found</returns>
        public static UIAProperty GetPropertyFromCode(int code)
        {
            if (propertyMapping == null)
            {
                InitPropertyMapping();
            }

            UIAProperty property;
            return propertyMapping.TryGetValue(code, out property) ? property : UIAProperty.Unknown;
        }

        /// <summary>
        /// Finds the code for a given property
        /// </summary>
        /// <param name="property">The property whose code to find</param>
        /// <returns>The code of the property</returns>
        public static int GetPropertyCode(UIAProperty property)
        {
            if (propertyMapping == null)
            {
                InitPropertyMapping();
            }

            // will throw if given an invalid code
            return propertyMapping.First(kv => kv.Value == property).Key;
        }

        /// <summary>
        /// Convert a code into a landmark
        /// </summary>
        /// <param name="code">The code to search for</param>
        /// <returns>The Landmark enum of the element, or Unknown if it is not found</returns>
        public static UIALandmarkType GetLandmarkTypeFromCode(int code)
        {
            if (landmarkTypeMapping == null)
            {
                InitLandmarkTypeMapping();
            }

            UIALandmarkType landmarkType;
            return landmarkTypeMapping.TryGetValue(code, out landmarkType) ? landmarkType : UIALandmarkType.Unknown;
        }

        /// <summary>
        /// Convert a WebDriverKey into its string representation
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <returns>A string representation of the given key</returns>
        public static string GetWebDriverKeyString(WebDriverKey key)
        {
            if (webDriverKeyMapping == null)
            {
                InitWebDriverKeyMapping();
            }

            return webDriverKeyMapping[key];
        }

        /// <summary>
        /// Initialize the control type mapping
        /// </summary>
        private static void InitControlTypeMapping()
        {
            controlTypeMapping = new Dictionary<int, UIAControlType>(42)
            {
                { -1, UIAControlType.Unknown },
                { 50000, UIAControlType.Button },
                { 50001, UIAControlType.Calendar },
                { 50002, UIAControlType.Checkbox },
                { 50003, UIAControlType.Combobox },
                { 50004, UIAControlType.Edit },
                { 50005, UIAControlType.Hyperlink },
                { 50006, UIAControlType.Image },
                { 50007, UIAControlType.Listitem },
                { 50008, UIAControlType.List },
                { 50009, UIAControlType.Menu },
                { 50010, UIAControlType.Menubar },
                { 50011, UIAControlType.Menuitem },
                { 50012, UIAControlType.Progressbar },
                { 50013, UIAControlType.Radiobutton },
                { 50014, UIAControlType.Scrollbar },
                { 50015, UIAControlType.Slider },
                { 50016, UIAControlType.Spinner },
                { 50017, UIAControlType.Statusbar },
                { 50018, UIAControlType.Tab },
                { 50019, UIAControlType.Tabitem },
                { 50020, UIAControlType.Text },
                { 50021, UIAControlType.Toolbar },
                { 50022, UIAControlType.Tooltip },
                { 50023, UIAControlType.Tree },
                { 50024, UIAControlType.Treeitem },
                { 50025, UIAControlType.Custom },
                { 50026, UIAControlType.Group },
                { 50027, UIAControlType.Thumb },
                { 50028, UIAControlType.Datagrid },
                { 50029, UIAControlType.Dataitem },
                { 50030, UIAControlType.Document },
                { 50031, UIAControlType.Splitbutton },
                { 50032, UIAControlType.Window },
                { 50033, UIAControlType.Pane },
                { 50034, UIAControlType.Header },
                { 50035, UIAControlType.Headeritem },
                { 50036, UIAControlType.Table },
                { 50037, UIAControlType.Titlebar },
                { 50038, UIAControlType.Separator },
                { 50039, UIAControlType.Semanticzoom },
                { 50040, UIAControlType.Appbar }
            };
        }

        /// <summary>
        /// Initialize the property mapping
        /// </summary>
        private static void InitPropertyMapping()
        {
            propertyMapping = new Dictionary<int, UIAProperty>(112)
            {
                { -1, UIAProperty.Unknown },
                { 30000, UIAProperty.RuntimeId },
                { 30001, UIAProperty.BoundingRectangle },
                { 30002, UIAProperty.ProcessId },
                { 30003, UIAProperty.ControlType },
                { 30004, UIAProperty.LocalizedControlType },
                { 30005, UIAProperty.Name },
                { 30006, UIAProperty.AcceleratorKey },
                { 30007, UIAProperty.AccessKey },
                { 30008, UIAProperty.HasKeyboardFocus },
                { 30009, UIAProperty.IsKeyboardFocusable },
                { 30010, UIAProperty.IsEnabled },
                { 30011, UIAProperty.AutomationId },
                { 30012, UIAProperty.ClassName },
                { 30013, UIAProperty.HelpText },
                { 30014, UIAProperty.ClickablePoint },
                { 30015, UIAProperty.Culture },
                { 30016, UIAProperty.IsControlElement },
                { 30017, UIAProperty.IsContentElement },
                { 30018, UIAProperty.LabeledBy },
                { 30019, UIAProperty.IsPassword },
                { 30020, UIAProperty.NativeWindowHandle },
                { 30021, UIAProperty.ItemType },
                { 30022, UIAProperty.IsOffscreen },
                { 30023, UIAProperty.Orientation },
                { 30024, UIAProperty.FrameworkId },
                { 30025, UIAProperty.IsRequiredForForm },
                { 30026, UIAProperty.ItemStatus },
                { 30027, UIAProperty.IsDockPatternAvailable },
                { 30028, UIAProperty.IsExpandCollapsePatternAvailable },
                { 30029, UIAProperty.IsGridItemPatternAvailable },
                { 30030, UIAProperty.IsGridPatternAvailable },
                { 30031, UIAProperty.IsInvokePatternAvailable },
                { 30032, UIAProperty.IsMultipleViewPatternAvailable },
                { 30033, UIAProperty.IsRangeValuePatternAvailable },
                { 30034, UIAProperty.IsScrollPatternAvailable },
                { 30035, UIAProperty.IsScrollItemPatternAvailable },
                { 30036, UIAProperty.IsSelectionItemPatternAvailable },
                { 30037, UIAProperty.IsSelectionPatternAvailable },
                { 30038, UIAProperty.IsTablePatternAvailable },
                { 30039, UIAProperty.IsTableItemPatternAvailable },
                { 30040, UIAProperty.IsTextPatternAvailable },
                { 30041, UIAProperty.IsTogglePatternAvailable },
                { 30042, UIAProperty.IsTransformPatternAvailable },
                { 30043, UIAProperty.IsValuePatternAvailable },
                { 30044, UIAProperty.IsWindowPatternAvailable },
                { 30045, UIAProperty.ValueValue },
                { 30046, UIAProperty.ValueIsReadOnly },
                { 30047, UIAProperty.RangeValueValue },
                { 30048, UIAProperty.RangeValueIsReadOnly },
                { 30049, UIAProperty.RangeValueMinimum },
                { 30050, UIAProperty.RangeValueMaximum },
                { 30051, UIAProperty.RangeValueLargeChange },
                { 30052, UIAProperty.RangeValueSmallChange },
                { 30053, UIAProperty.ScrollHorizontalScrollPercent },
                { 30054, UIAProperty.ScrollHorizontalViewSize },
                { 30055, UIAProperty.ScrollVerticalScrollPercent },
                { 30056, UIAProperty.ScrollVerticalViewSize },
                { 30057, UIAProperty.ScrollHorizontallyScrollable },
                { 30058, UIAProperty.ScrollVerticallyScrollable },
                { 30059, UIAProperty.SelectionSelection },
                { 30060, UIAProperty.SelectionCanSelectMultiple },
                { 30061, UIAProperty.SelectionIsSelectionRequired },
                { 30062, UIAProperty.GridRowCount },
                { 30063, UIAProperty.GridColumnCount },
                { 30064, UIAProperty.GridItemRow },
                { 30065, UIAProperty.GridItemColumn },
                { 30066, UIAProperty.GridItemRowSpan },
                { 30067, UIAProperty.GridItemColumnSpan },
                { 30068, UIAProperty.GridItemContainingGrid },
                { 30069, UIAProperty.DockDockPosition },
                { 30070, UIAProperty.ExpandCollapseExpandCollapseState },
                { 30071, UIAProperty.MultipleViewCurrentView },
                { 30072, UIAProperty.MultipleViewSupportedViews },
                { 30073, UIAProperty.WindowCanMaximize },
                { 30074, UIAProperty.WindowCanMinimize },
                { 30075, UIAProperty.WindowWindowVisualState },
                { 30076, UIAProperty.WindowWindowInteractionState },
                { 30077, UIAProperty.WindowIsModal },
                { 30078, UIAProperty.WindowIsTopmost },
                { 30079, UIAProperty.SelectionItemIsSelected },
                { 30080, UIAProperty.SelectionItemSelectionContainer },
                { 30081, UIAProperty.TableRowHeaders },
                { 30082, UIAProperty.TableColumnHeaders },
                { 30083, UIAProperty.TableRowOrColumnMajor },
                { 30084, UIAProperty.TableItemRowHeaderItems },
                { 30085, UIAProperty.TableItemColumnHeaderItems },
                { 30086, UIAProperty.ToggleToggleState },
                { 30087, UIAProperty.TransformCanMove },
                { 30088, UIAProperty.TransformCanResize },
                { 30089, UIAProperty.TransformCanRotate },
                { 30090, UIAProperty.IsLegacyIAccessiblePatternAvailable },
                { 30091, UIAProperty.LegacyIAccessibleChildId },
                { 30092, UIAProperty.LegacyIAccessibleName },
                { 30093, UIAProperty.LegacyIAccessibleValue },
                { 30094, UIAProperty.LegacyIAccessibleDescription },
                { 30095, UIAProperty.LegacyIAccessibleRole },
                { 30096, UIAProperty.LegacyIAccessibleState },
                { 30097, UIAProperty.LegacyIAccessibleHelp },
                { 30098, UIAProperty.LegacyIAccessibleKeyboardShortcut },
                { 30099, UIAProperty.LegacyIAccessibleSelection },
                { 30100, UIAProperty.LegacyIAccessibleDefaultAction },
                { 30101, UIAProperty.AriaRole },
                { 30102, UIAProperty.AriaProperties },
                { 30103, UIAProperty.IsDataValidForForm },
                { 30104, UIAProperty.ControllerFor },
                { 30105, UIAProperty.DescribedBy },
                { 30106, UIAProperty.FlowsTo },
                { 30107, UIAProperty.ProviderDescription },
                { 30108, UIAProperty.IsItemContainerPatternAvailable },
                { 30109, UIAProperty.IsVirtualizedItemPatternAvailable },
                { 30110, UIAProperty.IsSynchronizedInputPatternAvailable }
            };
        }

        /// <summary>
        /// Initialize the landmark mapping
        /// </summary>
        private static void InitLandmarkTypeMapping()
        {
            landmarkTypeMapping = new Dictionary<int, UIALandmarkType>(6)
            {
                { -1, UIALandmarkType.Unknown },
                { 80000, UIALandmarkType.Custom },
                { 80001, UIALandmarkType.Form },
                { 80002, UIALandmarkType.Main },
                { 80003, UIALandmarkType.Navigation },
                { 80004, UIALandmarkType.Search },
            };
        }

        /// <summary>
        /// Initialize the key mapping
        /// </summary>
        private static void InitWebDriverKeyMapping()
        {
            webDriverKeyMapping = new Dictionary<WebDriverKey, string>(58)
            {
                { WebDriverKey.Wait, null },
                { WebDriverKey.Null, '\uE000'.ToString() },
                { WebDriverKey.Cancel, '\uE001'.ToString() },
                { WebDriverKey.Help, '\uE002'.ToString() },
                { WebDriverKey.Back_space, '\uE003'.ToString() },
                { WebDriverKey.Tab, '\uE004'.ToString() },
                { WebDriverKey.Clear, '\uE005'.ToString() },
                { WebDriverKey.Return, '\uE006'.ToString() },
                { WebDriverKey.Enter, '\uE007'.ToString() },
                { WebDriverKey.Shift, '\uE008'.ToString() },
                { WebDriverKey.Control, '\uE009'.ToString() },
                { WebDriverKey.Alt, '\uE00A'.ToString() },
                { WebDriverKey.Pause, '\uE00B'.ToString() },
                { WebDriverKey.Escape, '\uE00C'.ToString() },
                { WebDriverKey.Space, '\uE00D'.ToString() },
                { WebDriverKey.Page_up, '\uE00E'.ToString() },
                { WebDriverKey.Page_down, '\uE00F'.ToString() },
                { WebDriverKey.End, '\uE010'.ToString() },
                { WebDriverKey.Home, '\uE011'.ToString() },
                { WebDriverKey.Arrow_left, '\uE012'.ToString() },
                { WebDriverKey.Arrow_up, '\uE013'.ToString() },
                { WebDriverKey.Arrow_right, '\uE014'.ToString() },
                { WebDriverKey.Arrow_down, '\uE015'.ToString() },
                { WebDriverKey.Insert, '\uE016'.ToString() },
                { WebDriverKey.Delete, '\uE017'.ToString() },
                { WebDriverKey.Semicolon, '\uE018'.ToString() },
                { WebDriverKey.Equals, '\uE019'.ToString() },
                { WebDriverKey.Numpad0, '\uE01A'.ToString() },
                { WebDriverKey.Numpad1, '\uE01B'.ToString() },
                { WebDriverKey.Numpad2, '\uE01C'.ToString() },
                { WebDriverKey.Numpad3, '\uE01D'.ToString() },
                { WebDriverKey.Numpad4, '\uE01E'.ToString() },
                { WebDriverKey.Numpad5, '\uE01F'.ToString() },
                { WebDriverKey.Numpad6, '\uE020'.ToString() },
                { WebDriverKey.Numpad7, '\uE021'.ToString() },
                { WebDriverKey.Numpad8, '\uE022'.ToString() },
                { WebDriverKey.Numpad9, '\uE023'.ToString() },
                { WebDriverKey.Multiply, '\uE024'.ToString() },
                { WebDriverKey.Add, '\uE025'.ToString() },
                { WebDriverKey.Separator, '\uE026'.ToString() },
                { WebDriverKey.Subtract, '\uE027'.ToString() },
                { WebDriverKey.Decimal, '\uE028'.ToString() },
                { WebDriverKey.Divide, '\uE029'.ToString() },
                { WebDriverKey.F1, '\uE031'.ToString() },
                { WebDriverKey.F2, '\uE032'.ToString() },
                { WebDriverKey.F3, '\uE033'.ToString() },
                { WebDriverKey.F4, '\uE034'.ToString() },
                { WebDriverKey.F5, '\uE035'.ToString() },
                { WebDriverKey.F6, '\uE036'.ToString() },
                { WebDriverKey.F7, '\uE037'.ToString() },
                { WebDriverKey.F8, '\uE038'.ToString() },
                { WebDriverKey.F9, '\uE039'.ToString() },
                { WebDriverKey.F10, '\uE03A'.ToString() },
                { WebDriverKey.F11, '\uE03B'.ToString() },
                { WebDriverKey.F12, '\uE03C'.ToString() },
                { WebDriverKey.Meta, '\uE03D'.ToString() },
                { WebDriverKey.Command, '\uE03D'.ToString() },
                { WebDriverKey.Zenkaku_hankaku, '\uE040'.ToString() },
            };
        }
    }
}
