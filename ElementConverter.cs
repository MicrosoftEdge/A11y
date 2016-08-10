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

            return controlTypeMapping.ContainsKey(code) ? controlTypeMapping[code] : UIAControlType.Unknown;
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

            return propertyMapping.ContainsKey(code) ? propertyMapping[code] : UIAProperty.Unknown;
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
            return propertyMapping.Keys.First(k => propertyMapping[k] == property);
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

            return landmarkTypeMapping.ContainsKey(code) ? landmarkTypeMapping[code] : UIALandmarkType.Unknown;
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
            var controlTypeMapping = new Dictionary<int, UIAControlType>();
            controlTypeMapping.Add(-1, UIAControlType.Unknown);
            controlTypeMapping.Add(50000, UIAControlType.Button);
            controlTypeMapping.Add(50001, UIAControlType.Calendar);
            controlTypeMapping.Add(50002, UIAControlType.Checkbox);
            controlTypeMapping.Add(50003, UIAControlType.Combobox);
            controlTypeMapping.Add(50004, UIAControlType.Edit);
            controlTypeMapping.Add(50005, UIAControlType.Hyperlink);
            controlTypeMapping.Add(50006, UIAControlType.Image);
            controlTypeMapping.Add(50007, UIAControlType.Listitem);
            controlTypeMapping.Add(50008, UIAControlType.List);
            controlTypeMapping.Add(50009, UIAControlType.Menu);
            controlTypeMapping.Add(50010, UIAControlType.Menubar);
            controlTypeMapping.Add(50011, UIAControlType.Menuitem);
            controlTypeMapping.Add(50012, UIAControlType.Progressbar);
            controlTypeMapping.Add(50013, UIAControlType.Radiobutton);
            controlTypeMapping.Add(50014, UIAControlType.Scrollbar);
            controlTypeMapping.Add(50015, UIAControlType.Slider);
            controlTypeMapping.Add(50016, UIAControlType.Spinner);
            controlTypeMapping.Add(50017, UIAControlType.Statusbar);
            controlTypeMapping.Add(50018, UIAControlType.Tab);
            controlTypeMapping.Add(50019, UIAControlType.Tabitem);
            controlTypeMapping.Add(50020, UIAControlType.Text);
            controlTypeMapping.Add(50021, UIAControlType.Toolbar);
            controlTypeMapping.Add(50022, UIAControlType.Tooltip);
            controlTypeMapping.Add(50023, UIAControlType.Tree);
            controlTypeMapping.Add(50024, UIAControlType.Treeitem);
            controlTypeMapping.Add(50025, UIAControlType.Custom);
            controlTypeMapping.Add(50026, UIAControlType.Group);
            controlTypeMapping.Add(50027, UIAControlType.Thumb);
            controlTypeMapping.Add(50028, UIAControlType.Datagrid);
            controlTypeMapping.Add(50029, UIAControlType.Dataitem);
            controlTypeMapping.Add(50030, UIAControlType.Document);
            controlTypeMapping.Add(50031, UIAControlType.Splitbutton);
            controlTypeMapping.Add(50032, UIAControlType.Window);
            controlTypeMapping.Add(50033, UIAControlType.Pane);
            controlTypeMapping.Add(50034, UIAControlType.Header);
            controlTypeMapping.Add(50035, UIAControlType.Headeritem);
            controlTypeMapping.Add(50036, UIAControlType.Table);
            controlTypeMapping.Add(50037, UIAControlType.Titlebar);
            controlTypeMapping.Add(50038, UIAControlType.Separator);
            controlTypeMapping.Add(50039, UIAControlType.Semanticzoom);
            controlTypeMapping.Add(50040, UIAControlType.Appbar);

            ElementConverter.controlTypeMapping = controlTypeMapping;
        }

        /// <summary>
        /// Initialize the property mapping
        /// </summary>
        private static void InitPropertyMapping()
        {
            var propertyMapping = new Dictionary<int, UIAProperty>();
            propertyMapping.Add(-1, UIAProperty.Unknown);
            propertyMapping.Add(30000, UIAProperty.RuntimeId);
            propertyMapping.Add(30001, UIAProperty.BoundingRectangle);
            propertyMapping.Add(30002, UIAProperty.ProcessId);
            propertyMapping.Add(30003, UIAProperty.ControlType);
            propertyMapping.Add(30004, UIAProperty.LocalizedControlType);
            propertyMapping.Add(30005, UIAProperty.Name);
            propertyMapping.Add(30006, UIAProperty.AcceleratorKey);
            propertyMapping.Add(30007, UIAProperty.AccessKey);
            propertyMapping.Add(30008, UIAProperty.HasKeyboardFocus);
            propertyMapping.Add(30009, UIAProperty.IsKeyboardFocusable);
            propertyMapping.Add(30010, UIAProperty.IsEnabled);
            propertyMapping.Add(30011, UIAProperty.AutomationId);
            propertyMapping.Add(30012, UIAProperty.ClassName);
            propertyMapping.Add(30013, UIAProperty.HelpText);
            propertyMapping.Add(30014, UIAProperty.ClickablePoint);
            propertyMapping.Add(30015, UIAProperty.Culture);
            propertyMapping.Add(30016, UIAProperty.IsControlElement);
            propertyMapping.Add(30017, UIAProperty.IsContentElement);
            propertyMapping.Add(30018, UIAProperty.LabeledBy);
            propertyMapping.Add(30019, UIAProperty.IsPassword);
            propertyMapping.Add(30020, UIAProperty.NativeWindowHandle);
            propertyMapping.Add(30021, UIAProperty.ItemType);
            propertyMapping.Add(30022, UIAProperty.IsOffscreen);
            propertyMapping.Add(30023, UIAProperty.Orientation);
            propertyMapping.Add(30024, UIAProperty.FrameworkId);
            propertyMapping.Add(30025, UIAProperty.IsRequiredForForm);
            propertyMapping.Add(30026, UIAProperty.ItemStatus);
            propertyMapping.Add(30027, UIAProperty.IsDockPatternAvailable);
            propertyMapping.Add(30028, UIAProperty.IsExpandCollapsePatternAvailable);
            propertyMapping.Add(30029, UIAProperty.IsGridItemPatternAvailable);
            propertyMapping.Add(30030, UIAProperty.IsGridPatternAvailable);
            propertyMapping.Add(30031, UIAProperty.IsInvokePatternAvailable);
            propertyMapping.Add(30032, UIAProperty.IsMultipleViewPatternAvailable);
            propertyMapping.Add(30033, UIAProperty.IsRangeValuePatternAvailable);
            propertyMapping.Add(30034, UIAProperty.IsScrollPatternAvailable);
            propertyMapping.Add(30035, UIAProperty.IsScrollItemPatternAvailable);
            propertyMapping.Add(30036, UIAProperty.IsSelectionItemPatternAvailable);
            propertyMapping.Add(30037, UIAProperty.IsSelectionPatternAvailable);
            propertyMapping.Add(30038, UIAProperty.IsTablePatternAvailable);
            propertyMapping.Add(30039, UIAProperty.IsTableItemPatternAvailable);
            propertyMapping.Add(30040, UIAProperty.IsTextPatternAvailable);
            propertyMapping.Add(30041, UIAProperty.IsTogglePatternAvailable);
            propertyMapping.Add(30042, UIAProperty.IsTransformPatternAvailable);
            propertyMapping.Add(30043, UIAProperty.IsValuePatternAvailable);
            propertyMapping.Add(30044, UIAProperty.IsWindowPatternAvailable);
            propertyMapping.Add(30045, UIAProperty.ValueValue);
            propertyMapping.Add(30046, UIAProperty.ValueIsReadOnly);
            propertyMapping.Add(30047, UIAProperty.RangeValueValue);
            propertyMapping.Add(30048, UIAProperty.RangeValueIsReadOnly);
            propertyMapping.Add(30049, UIAProperty.RangeValueMinimum);
            propertyMapping.Add(30050, UIAProperty.RangeValueMaximum);
            propertyMapping.Add(30051, UIAProperty.RangeValueLargeChange);
            propertyMapping.Add(30052, UIAProperty.RangeValueSmallChange);
            propertyMapping.Add(30053, UIAProperty.ScrollHorizontalScrollPercent);
            propertyMapping.Add(30054, UIAProperty.ScrollHorizontalViewSize);
            propertyMapping.Add(30055, UIAProperty.ScrollVerticalScrollPercent);
            propertyMapping.Add(30056, UIAProperty.ScrollVerticalViewSize);
            propertyMapping.Add(30057, UIAProperty.ScrollHorizontallyScrollable);
            propertyMapping.Add(30058, UIAProperty.ScrollVerticallyScrollable);
            propertyMapping.Add(30059, UIAProperty.SelectionSelection);
            propertyMapping.Add(30060, UIAProperty.SelectionCanSelectMultiple);
            propertyMapping.Add(30061, UIAProperty.SelectionIsSelectionRequired);
            propertyMapping.Add(30062, UIAProperty.GridRowCount);
            propertyMapping.Add(30063, UIAProperty.GridColumnCount);
            propertyMapping.Add(30064, UIAProperty.GridItemRow);
            propertyMapping.Add(30065, UIAProperty.GridItemColumn);
            propertyMapping.Add(30066, UIAProperty.GridItemRowSpan);
            propertyMapping.Add(30067, UIAProperty.GridItemColumnSpan);
            propertyMapping.Add(30068, UIAProperty.GridItemContainingGrid);
            propertyMapping.Add(30069, UIAProperty.DockDockPosition);
            propertyMapping.Add(30070, UIAProperty.ExpandCollapseExpandCollapseState);
            propertyMapping.Add(30071, UIAProperty.MultipleViewCurrentView);
            propertyMapping.Add(30072, UIAProperty.MultipleViewSupportedViews);
            propertyMapping.Add(30073, UIAProperty.WindowCanMaximize);
            propertyMapping.Add(30074, UIAProperty.WindowCanMinimize);
            propertyMapping.Add(30075, UIAProperty.WindowWindowVisualState);
            propertyMapping.Add(30076, UIAProperty.WindowWindowInteractionState);
            propertyMapping.Add(30077, UIAProperty.WindowIsModal);
            propertyMapping.Add(30078, UIAProperty.WindowIsTopmost);
            propertyMapping.Add(30079, UIAProperty.SelectionItemIsSelected);
            propertyMapping.Add(30080, UIAProperty.SelectionItemSelectionContainer);
            propertyMapping.Add(30081, UIAProperty.TableRowHeaders);
            propertyMapping.Add(30082, UIAProperty.TableColumnHeaders);
            propertyMapping.Add(30083, UIAProperty.TableRowOrColumnMajor);
            propertyMapping.Add(30084, UIAProperty.TableItemRowHeaderItems);
            propertyMapping.Add(30085, UIAProperty.TableItemColumnHeaderItems);
            propertyMapping.Add(30086, UIAProperty.ToggleToggleState);
            propertyMapping.Add(30087, UIAProperty.TransformCanMove);
            propertyMapping.Add(30088, UIAProperty.TransformCanResize);
            propertyMapping.Add(30089, UIAProperty.TransformCanRotate);
            propertyMapping.Add(30090, UIAProperty.IsLegacyIAccessiblePatternAvailable);
            propertyMapping.Add(30091, UIAProperty.LegacyIAccessibleChildId);
            propertyMapping.Add(30092, UIAProperty.LegacyIAccessibleName);
            propertyMapping.Add(30093, UIAProperty.LegacyIAccessibleValue);
            propertyMapping.Add(30094, UIAProperty.LegacyIAccessibleDescription);
            propertyMapping.Add(30095, UIAProperty.LegacyIAccessibleRole);
            propertyMapping.Add(30096, UIAProperty.LegacyIAccessibleState);
            propertyMapping.Add(30097, UIAProperty.LegacyIAccessibleHelp);
            propertyMapping.Add(30098, UIAProperty.LegacyIAccessibleKeyboardShortcut);
            propertyMapping.Add(30099, UIAProperty.LegacyIAccessibleSelection);
            propertyMapping.Add(30100, UIAProperty.LegacyIAccessibleDefaultAction);
            propertyMapping.Add(30101, UIAProperty.AriaRole);
            propertyMapping.Add(30102, UIAProperty.AriaProperties);
            propertyMapping.Add(30103, UIAProperty.IsDataValidForForm);
            propertyMapping.Add(30104, UIAProperty.ControllerFor);
            propertyMapping.Add(30105, UIAProperty.DescribedBy);
            propertyMapping.Add(30106, UIAProperty.FlowsTo);
            propertyMapping.Add(30107, UIAProperty.ProviderDescription);
            propertyMapping.Add(30108, UIAProperty.IsItemContainerPatternAvailable);
            propertyMapping.Add(30109, UIAProperty.IsVirtualizedItemPatternAvailable);
            propertyMapping.Add(30110, UIAProperty.IsSynchronizedInputPatternAvailable);

            ElementConverter.propertyMapping = propertyMapping;
        }

        /// <summary>
        /// Initialize the landmark mapping
        /// </summary>
        private static void InitLandmarkTypeMapping()
        {
            var landmarkTypeMapping = new Dictionary<int, UIALandmarkType>();
            landmarkTypeMapping.Add(-1, UIALandmarkType.Unknown);
            landmarkTypeMapping.Add(80000, UIALandmarkType.Custom);
            landmarkTypeMapping.Add(80001, UIALandmarkType.Form);
            landmarkTypeMapping.Add(80002, UIALandmarkType.Main);
            landmarkTypeMapping.Add(80003, UIALandmarkType.Navigation);
            landmarkTypeMapping.Add(80004, UIALandmarkType.Search);
            ElementConverter.landmarkTypeMapping = landmarkTypeMapping;
        }

        /// <summary>
        /// Initialize the key mapping
        /// </summary>
        private static void InitWebDriverKeyMapping()
        {
            var webDriverKeyMapping = new Dictionary<WebDriverKey, string>();

            webDriverKeyMapping.Add(WebDriverKey.Wait, null);
            webDriverKeyMapping.Add(WebDriverKey.Null, '\uE000'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Cancel, '\uE001'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Help, '\uE002'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Back_space, '\uE003'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Tab, '\uE004'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Clear, '\uE005'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Return, '\uE006'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Enter, '\uE007'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Shift, '\uE008'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Control, '\uE009'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Alt, '\uE00A'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Pause, '\uE00B'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Escape, '\uE00C'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Space, '\uE00D'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Page_up, '\uE00E'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Page_down, '\uE00F'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.End, '\uE010'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Home, '\uE011'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Arrow_left, '\uE012'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Arrow_up, '\uE013'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Arrow_right, '\uE014'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Arrow_down, '\uE015'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Insert, '\uE016'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Delete, '\uE017'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Semicolon, '\uE018'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Equals, '\uE019'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Numpad0, '\uE01A'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Numpad1, '\uE01B'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Numpad2, '\uE01C'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Numpad3, '\uE01D'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Numpad4, '\uE01E'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Numpad5, '\uE01F'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Numpad6, '\uE020'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Numpad7, '\uE021'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Numpad8, '\uE022'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Numpad9, '\uE023'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Multiply, '\uE024'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Add, '\uE025'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Separator, '\uE026'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Subtract, '\uE027'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Decimal, '\uE028'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Divide, '\uE029'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.F1, '\uE031'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.F2, '\uE032'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.F3, '\uE033'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.F4, '\uE034'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.F5, '\uE035'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.F6, '\uE036'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.F7, '\uE037'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.F8, '\uE038'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.F9, '\uE039'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.F10, '\uE03A'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.F11, '\uE03B'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.F12, '\uE03C'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Meta, '\uE03D'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Command, '\uE03D'.ToString());
            webDriverKeyMapping.Add(WebDriverKey.Zenkaku_hankaku, '\uE040'.ToString());

            ElementConverter.webDriverKeyMapping = webDriverKeyMapping;
        }
    }
}
