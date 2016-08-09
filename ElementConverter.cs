namespace Microsoft.Edge.A11y
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A tool to convert element names into codes and back again
    /// </summary>
    public static class ElementConverter
    {
        private static Dictionary<int, UIAControlType> _ControlTypeMapping;
        private static Dictionary<int, UIAProperty> _PropertyMapping;
        private static Dictionary<int, UIALandmarkType> _LandmarkTypeMapping;
        private static Dictionary<WebDriverKey, string> _WebDriverKeyMapping;

        private static void InitControlTypeMapping()
        {
            var ControlTypeMapping = new Dictionary<int, UIAControlType>();
            ControlTypeMapping.Add(-1, UIAControlType.Unknown);
            ControlTypeMapping.Add(50000, UIAControlType.Button);
            ControlTypeMapping.Add(50001, UIAControlType.Calendar);
            ControlTypeMapping.Add(50002, UIAControlType.Checkbox);
            ControlTypeMapping.Add(50003, UIAControlType.Combobox);
            ControlTypeMapping.Add(50004, UIAControlType.Edit);
            ControlTypeMapping.Add(50005, UIAControlType.Hyperlink);
            ControlTypeMapping.Add(50006, UIAControlType.Image);
            ControlTypeMapping.Add(50007, UIAControlType.Listitem);
            ControlTypeMapping.Add(50008, UIAControlType.List);
            ControlTypeMapping.Add(50009, UIAControlType.Menu);
            ControlTypeMapping.Add(50010, UIAControlType.Menubar);
            ControlTypeMapping.Add(50011, UIAControlType.Menuitem);
            ControlTypeMapping.Add(50012, UIAControlType.Progressbar);
            ControlTypeMapping.Add(50013, UIAControlType.Radiobutton);
            ControlTypeMapping.Add(50014, UIAControlType.Scrollbar);
            ControlTypeMapping.Add(50015, UIAControlType.Slider);
            ControlTypeMapping.Add(50016, UIAControlType.Spinner);
            ControlTypeMapping.Add(50017, UIAControlType.Statusbar);
            ControlTypeMapping.Add(50018, UIAControlType.Tab);
            ControlTypeMapping.Add(50019, UIAControlType.Tabitem);
            ControlTypeMapping.Add(50020, UIAControlType.Text);
            ControlTypeMapping.Add(50021, UIAControlType.Toolbar);
            ControlTypeMapping.Add(50022, UIAControlType.Tooltip);
            ControlTypeMapping.Add(50023, UIAControlType.Tree);
            ControlTypeMapping.Add(50024, UIAControlType.Treeitem);
            ControlTypeMapping.Add(50025, UIAControlType.Custom);
            ControlTypeMapping.Add(50026, UIAControlType.Group);
            ControlTypeMapping.Add(50027, UIAControlType.Thumb);
            ControlTypeMapping.Add(50028, UIAControlType.Datagrid);
            ControlTypeMapping.Add(50029, UIAControlType.Dataitem);
            ControlTypeMapping.Add(50030, UIAControlType.Document);
            ControlTypeMapping.Add(50031, UIAControlType.Splitbutton);
            ControlTypeMapping.Add(50032, UIAControlType.Window);
            ControlTypeMapping.Add(50033, UIAControlType.Pane);
            ControlTypeMapping.Add(50034, UIAControlType.Header);
            ControlTypeMapping.Add(50035, UIAControlType.Headeritem);
            ControlTypeMapping.Add(50036, UIAControlType.Table);
            ControlTypeMapping.Add(50037, UIAControlType.Titlebar);
            ControlTypeMapping.Add(50038, UIAControlType.Separator);
            ControlTypeMapping.Add(50039, UIAControlType.Semanticzoom);
            ControlTypeMapping.Add(50040, UIAControlType.Appbar);

            _ControlTypeMapping = ControlTypeMapping;
        }

        private static void InitPropertyMapping()
        {
            var PropertyMapping = new Dictionary<int, UIAProperty>();
            PropertyMapping.Add(-1, UIAProperty.Unknown);
            PropertyMapping.Add(30000, UIAProperty.RuntimeId);
            PropertyMapping.Add(30001, UIAProperty.BoundingRectangle);
            PropertyMapping.Add(30002, UIAProperty.ProcessId);
            PropertyMapping.Add(30003, UIAProperty.ControlType);
            PropertyMapping.Add(30004, UIAProperty.LocalizedControlType);
            PropertyMapping.Add(30005, UIAProperty.Name);
            PropertyMapping.Add(30006, UIAProperty.AcceleratorKey);
            PropertyMapping.Add(30007, UIAProperty.AccessKey);
            PropertyMapping.Add(30008, UIAProperty.HasKeyboardFocus);
            PropertyMapping.Add(30009, UIAProperty.IsKeyboardFocusable);
            PropertyMapping.Add(30010, UIAProperty.IsEnabled);
            PropertyMapping.Add(30011, UIAProperty.AutomationId);
            PropertyMapping.Add(30012, UIAProperty.ClassName);
            PropertyMapping.Add(30013, UIAProperty.HelpText);
            PropertyMapping.Add(30014, UIAProperty.ClickablePoint);
            PropertyMapping.Add(30015, UIAProperty.Culture);
            PropertyMapping.Add(30016, UIAProperty.IsControlElement);
            PropertyMapping.Add(30017, UIAProperty.IsContentElement);
            PropertyMapping.Add(30018, UIAProperty.LabeledBy);
            PropertyMapping.Add(30019, UIAProperty.IsPassword);
            PropertyMapping.Add(30020, UIAProperty.NativeWindowHandle);
            PropertyMapping.Add(30021, UIAProperty.ItemType);
            PropertyMapping.Add(30022, UIAProperty.IsOffscreen);
            PropertyMapping.Add(30023, UIAProperty.Orientation);
            PropertyMapping.Add(30024, UIAProperty.FrameworkId);
            PropertyMapping.Add(30025, UIAProperty.IsRequiredForForm);
            PropertyMapping.Add(30026, UIAProperty.ItemStatus);
            PropertyMapping.Add(30027, UIAProperty.IsDockPatternAvailable);
            PropertyMapping.Add(30028, UIAProperty.IsExpandCollapsePatternAvailable);
            PropertyMapping.Add(30029, UIAProperty.IsGridItemPatternAvailable);
            PropertyMapping.Add(30030, UIAProperty.IsGridPatternAvailable);
            PropertyMapping.Add(30031, UIAProperty.IsInvokePatternAvailable);
            PropertyMapping.Add(30032, UIAProperty.IsMultipleViewPatternAvailable);
            PropertyMapping.Add(30033, UIAProperty.IsRangeValuePatternAvailable);
            PropertyMapping.Add(30034, UIAProperty.IsScrollPatternAvailable);
            PropertyMapping.Add(30035, UIAProperty.IsScrollItemPatternAvailable);
            PropertyMapping.Add(30036, UIAProperty.IsSelectionItemPatternAvailable);
            PropertyMapping.Add(30037, UIAProperty.IsSelectionPatternAvailable);
            PropertyMapping.Add(30038, UIAProperty.IsTablePatternAvailable);
            PropertyMapping.Add(30039, UIAProperty.IsTableItemPatternAvailable);
            PropertyMapping.Add(30040, UIAProperty.IsTextPatternAvailable);
            PropertyMapping.Add(30041, UIAProperty.IsTogglePatternAvailable);
            PropertyMapping.Add(30042, UIAProperty.IsTransformPatternAvailable);
            PropertyMapping.Add(30043, UIAProperty.IsValuePatternAvailable);
            PropertyMapping.Add(30044, UIAProperty.IsWindowPatternAvailable);
            PropertyMapping.Add(30045, UIAProperty.ValueValue);
            PropertyMapping.Add(30046, UIAProperty.ValueIsReadOnly);
            PropertyMapping.Add(30047, UIAProperty.RangeValueValue);
            PropertyMapping.Add(30048, UIAProperty.RangeValueIsReadOnly);
            PropertyMapping.Add(30049, UIAProperty.RangeValueMinimum);
            PropertyMapping.Add(30050, UIAProperty.RangeValueMaximum);
            PropertyMapping.Add(30051, UIAProperty.RangeValueLargeChange);
            PropertyMapping.Add(30052, UIAProperty.RangeValueSmallChange);
            PropertyMapping.Add(30053, UIAProperty.ScrollHorizontalScrollPercent);
            PropertyMapping.Add(30054, UIAProperty.ScrollHorizontalViewSize);
            PropertyMapping.Add(30055, UIAProperty.ScrollVerticalScrollPercent);
            PropertyMapping.Add(30056, UIAProperty.ScrollVerticalViewSize);
            PropertyMapping.Add(30057, UIAProperty.ScrollHorizontallyScrollable);
            PropertyMapping.Add(30058, UIAProperty.ScrollVerticallyScrollable);
            PropertyMapping.Add(30059, UIAProperty.SelectionSelection);
            PropertyMapping.Add(30060, UIAProperty.SelectionCanSelectMultiple);
            PropertyMapping.Add(30061, UIAProperty.SelectionIsSelectionRequired);
            PropertyMapping.Add(30062, UIAProperty.GridRowCount);
            PropertyMapping.Add(30063, UIAProperty.GridColumnCount);
            PropertyMapping.Add(30064, UIAProperty.GridItemRow);
            PropertyMapping.Add(30065, UIAProperty.GridItemColumn);
            PropertyMapping.Add(30066, UIAProperty.GridItemRowSpan);
            PropertyMapping.Add(30067, UIAProperty.GridItemColumnSpan);
            PropertyMapping.Add(30068, UIAProperty.GridItemContainingGrid);
            PropertyMapping.Add(30069, UIAProperty.DockDockPosition);
            PropertyMapping.Add(30070, UIAProperty.ExpandCollapseExpandCollapseState);
            PropertyMapping.Add(30071, UIAProperty.MultipleViewCurrentView);
            PropertyMapping.Add(30072, UIAProperty.MultipleViewSupportedViews);
            PropertyMapping.Add(30073, UIAProperty.WindowCanMaximize);
            PropertyMapping.Add(30074, UIAProperty.WindowCanMinimize);
            PropertyMapping.Add(30075, UIAProperty.WindowWindowVisualState);
            PropertyMapping.Add(30076, UIAProperty.WindowWindowInteractionState);
            PropertyMapping.Add(30077, UIAProperty.WindowIsModal);
            PropertyMapping.Add(30078, UIAProperty.WindowIsTopmost);
            PropertyMapping.Add(30079, UIAProperty.SelectionItemIsSelected);
            PropertyMapping.Add(30080, UIAProperty.SelectionItemSelectionContainer);
            PropertyMapping.Add(30081, UIAProperty.TableRowHeaders);
            PropertyMapping.Add(30082, UIAProperty.TableColumnHeaders);
            PropertyMapping.Add(30083, UIAProperty.TableRowOrColumnMajor);
            PropertyMapping.Add(30084, UIAProperty.TableItemRowHeaderItems);
            PropertyMapping.Add(30085, UIAProperty.TableItemColumnHeaderItems);
            PropertyMapping.Add(30086, UIAProperty.ToggleToggleState);
            PropertyMapping.Add(30087, UIAProperty.TransformCanMove);
            PropertyMapping.Add(30088, UIAProperty.TransformCanResize);
            PropertyMapping.Add(30089, UIAProperty.TransformCanRotate);
            PropertyMapping.Add(30090, UIAProperty.IsLegacyIAccessiblePatternAvailable);
            PropertyMapping.Add(30091, UIAProperty.LegacyIAccessibleChildId);
            PropertyMapping.Add(30092, UIAProperty.LegacyIAccessibleName);
            PropertyMapping.Add(30093, UIAProperty.LegacyIAccessibleValue);
            PropertyMapping.Add(30094, UIAProperty.LegacyIAccessibleDescription);
            PropertyMapping.Add(30095, UIAProperty.LegacyIAccessibleRole);
            PropertyMapping.Add(30096, UIAProperty.LegacyIAccessibleState);
            PropertyMapping.Add(30097, UIAProperty.LegacyIAccessibleHelp);
            PropertyMapping.Add(30098, UIAProperty.LegacyIAccessibleKeyboardShortcut);
            PropertyMapping.Add(30099, UIAProperty.LegacyIAccessibleSelection);
            PropertyMapping.Add(30100, UIAProperty.LegacyIAccessibleDefaultAction);
            PropertyMapping.Add(30101, UIAProperty.AriaRole);
            PropertyMapping.Add(30102, UIAProperty.AriaProperties);
            PropertyMapping.Add(30103, UIAProperty.IsDataValidForForm);
            PropertyMapping.Add(30104, UIAProperty.ControllerFor);
            PropertyMapping.Add(30105, UIAProperty.DescribedBy);
            PropertyMapping.Add(30106, UIAProperty.FlowsTo);
            PropertyMapping.Add(30107, UIAProperty.ProviderDescription);
            PropertyMapping.Add(30108, UIAProperty.IsItemContainerPatternAvailable);
            PropertyMapping.Add(30109, UIAProperty.IsVirtualizedItemPatternAvailable);
            PropertyMapping.Add(30110, UIAProperty.IsSynchronizedInputPatternAvailable);

            _PropertyMapping = PropertyMapping;
        }

        private static void InitLandmarkTypeMapping()
        {
            var LandmarkTypeMapping = new Dictionary<int, UIALandmarkType>();
            LandmarkTypeMapping.Add(-1, UIALandmarkType.Unknown);
            LandmarkTypeMapping.Add(80000, UIALandmarkType.Custom);
            LandmarkTypeMapping.Add(80001, UIALandmarkType.Form);
            LandmarkTypeMapping.Add(80002, UIALandmarkType.Main);
            LandmarkTypeMapping.Add(80003, UIALandmarkType.Navigation);
            LandmarkTypeMapping.Add(80004, UIALandmarkType.Search);
            _LandmarkTypeMapping = LandmarkTypeMapping;
        }

        private static void InitWebDriverKeyMapping()
        {
            var WebDriverKeyMapping = new Dictionary<WebDriverKey, string>();

            WebDriverKeyMapping.Add(WebDriverKey.Wait, null);
            WebDriverKeyMapping.Add(WebDriverKey.Null, '\uE000'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Cancel, '\uE001'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Help, '\uE002'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Back_space, '\uE003'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Tab, '\uE004'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Clear, '\uE005'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Return, '\uE006'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Enter, '\uE007'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Shift, '\uE008'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Control, '\uE009'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Alt, '\uE00A'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Pause, '\uE00B'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Escape, '\uE00C'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Space, '\uE00D'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Page_up, '\uE00E'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Page_down, '\uE00F'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.End, '\uE010'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Home, '\uE011'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Arrow_left, '\uE012'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Arrow_up, '\uE013'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Arrow_right, '\uE014'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Arrow_down, '\uE015'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Insert, '\uE016'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Delete, '\uE017'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Semicolon, '\uE018'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Equals, '\uE019'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Numpad0, '\uE01A'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Numpad1, '\uE01B'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Numpad2, '\uE01C'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Numpad3, '\uE01D'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Numpad4, '\uE01E'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Numpad5, '\uE01F'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Numpad6, '\uE020'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Numpad7, '\uE021'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Numpad8, '\uE022'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Numpad9, '\uE023'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Multiply, '\uE024'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Add, '\uE025'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Separator, '\uE026'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Subtract, '\uE027'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Decimal, '\uE028'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Divide, '\uE029'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.F1, '\uE031'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.F2, '\uE032'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.F3, '\uE033'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.F4, '\uE034'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.F5, '\uE035'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.F6, '\uE036'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.F7, '\uE037'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.F8, '\uE038'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.F9, '\uE039'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.F10, '\uE03A'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.F11, '\uE03B'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.F12, '\uE03C'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Meta, '\uE03D'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Command, '\uE03D'.ToString());
            WebDriverKeyMapping.Add(WebDriverKey.Zenkaku_hankaku, '\uE040'.ToString());

            _WebDriverKeyMapping = WebDriverKeyMapping;
        }

        /// <summary>
        /// Convert a code into an element name
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static UIAControlType GetControlTypeFromCode(int code)
        {
            if (_ControlTypeMapping == null)
            {
                InitControlTypeMapping();
            }
            return _ControlTypeMapping.ContainsKey(code) ? _ControlTypeMapping[code] : UIAControlType.Unknown;
        }

        /// <summary>
        /// Convert a code into a property
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static UIAProperty GetPropertyFromCode(int code)
        {
            if (_PropertyMapping == null)
            {
                InitPropertyMapping();
            }
            return _PropertyMapping.ContainsKey(code) ? _PropertyMapping[code] : UIAProperty.Unknown;
        }

        public static int GetPropertyCode(UIAProperty property)
        {
            if (_PropertyMapping == null)
            {
                InitPropertyMapping();
            }
            //will throw if given an invalid code
            return _PropertyMapping.Keys.First(k => _PropertyMapping[k] == property);
        }

        /// <summary>
        /// Convert a code into a landmark
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static UIALandmarkType GetLandmarkTypeFromCode(int code)
        {
            if (_LandmarkTypeMapping == null)
            {
                InitLandmarkTypeMapping();
            }
            return _LandmarkTypeMapping.ContainsKey(code) ? _LandmarkTypeMapping[code] : UIALandmarkType.Unknown;
        }

        /// <summary>
        /// Convert a WebDriverKey into its string representation
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetWebDriverKeyString(WebDriverKey key)
        {
            if (_WebDriverKeyMapping == null)
            {
                InitWebDriverKeyMapping();
            }
            return _WebDriverKeyMapping[key];
        }

        public enum UIAControlType
        {
            Unknown,
            Button,
            Calendar,
            Checkbox,
            Combobox,
            Edit,
            Hyperlink,
            Image,
            Listitem,
            List,
            Menu,
            Menubar,
            Menuitem,
            Progressbar,
            Radiobutton,
            Scrollbar,
            Slider,
            Spinner,
            Statusbar,
            Tab,
            Tabitem,
            Text,
            Toolbar,
            Tooltip,
            Tree,
            Treeitem,
            Custom,
            Group,
            Thumb,
            Datagrid,
            Dataitem,
            Document,
            Splitbutton,
            Window,
            Pane,
            Header,
            Headeritem,
            Table,
            Titlebar,
            Separator,
            Semanticzoom,
            Appbar,
        }

        public enum UIAProperty
        {
            Unknown,
            RuntimeId,
            BoundingRectangle,
            ProcessId,
            ControlType,
            LocalizedControlType,
            Name,
            AcceleratorKey,
            AccessKey,
            HasKeyboardFocus,
            IsKeyboardFocusable,
            IsEnabled,
            AutomationId,
            ClassName,
            HelpText,
            ClickablePoint,
            Culture,
            IsControlElement,
            IsContentElement,
            LabeledBy,
            IsPassword,
            NativeWindowHandle,
            ItemType,
            IsOffscreen,
            Orientation,
            FrameworkId,
            IsRequiredForForm,
            ItemStatus,
            IsDockPatternAvailable,
            IsExpandCollapsePatternAvailable,
            IsGridItemPatternAvailable,
            IsGridPatternAvailable,
            IsInvokePatternAvailable,
            IsMultipleViewPatternAvailable,
            IsRangeValuePatternAvailable,
            IsScrollPatternAvailable,
            IsScrollItemPatternAvailable,
            IsSelectionItemPatternAvailable,
            IsSelectionPatternAvailable,
            IsTablePatternAvailable,
            IsTableItemPatternAvailable,
            IsTextPatternAvailable,
            IsTogglePatternAvailable,
            IsTransformPatternAvailable,
            IsValuePatternAvailable,
            IsWindowPatternAvailable,
            ValueValue,
            ValueIsReadOnly,
            RangeValueValue,
            RangeValueIsReadOnly,
            RangeValueMinimum,
            RangeValueMaximum,
            RangeValueLargeChange,
            RangeValueSmallChange,
            ScrollHorizontalScrollPercent,
            ScrollHorizontalViewSize,
            ScrollVerticalScrollPercent,
            ScrollVerticalViewSize,
            ScrollHorizontallyScrollable,
            ScrollVerticallyScrollable,
            SelectionSelection,
            SelectionCanSelectMultiple,
            SelectionIsSelectionRequired,
            GridRowCount,
            GridColumnCount,
            GridItemRow,
            GridItemColumn,
            GridItemRowSpan,
            GridItemColumnSpan,
            GridItemContainingGrid,
            DockDockPosition,
            ExpandCollapseExpandCollapseState,
            MultipleViewCurrentView,
            MultipleViewSupportedViews,
            WindowCanMaximize,
            WindowCanMinimize,
            WindowWindowVisualState,
            WindowWindowInteractionState,
            WindowIsModal,
            WindowIsTopmost,
            SelectionItemIsSelected,
            SelectionItemSelectionContainer,
            TableRowHeaders,
            TableColumnHeaders,
            TableRowOrColumnMajor,
            TableItemRowHeaderItems,
            TableItemColumnHeaderItems,
            ToggleToggleState,
            TransformCanMove,
            TransformCanResize,
            TransformCanRotate,
            IsLegacyIAccessiblePatternAvailable,
            LegacyIAccessibleChildId,
            LegacyIAccessibleName,
            LegacyIAccessibleValue,
            LegacyIAccessibleDescription,
            LegacyIAccessibleRole,
            LegacyIAccessibleState,
            LegacyIAccessibleHelp,
            LegacyIAccessibleKeyboardShortcut,
            LegacyIAccessibleSelection,
            LegacyIAccessibleDefaultAction,
            AriaRole,
            AriaProperties,
            IsDataValidForForm,
            ControllerFor,
            DescribedBy,
            FlowsTo,
            ProviderDescription,
            IsItemContainerPatternAvailable,
            IsVirtualizedItemPatternAvailable,
            IsSynchronizedInputPatternAvailable,
        }

        public enum UIALandmarkType
        {
            Unknown,
            Custom,
            Form,
            Main,
            Navigation,
            Search
        }

        public enum WebDriverKey
        {
            Null,
            Cancel,
            Help,
            Back_space,
            Tab,
            Clear,
            Return,
            Enter,
            Shift,
            Control,
            Alt,
            Pause,
            Escape,
            Space,
            Page_up,
            Page_down,
            End,
            Home,
            Arrow_left,
            Arrow_up,
            Arrow_right,
            Arrow_down,
            Insert,
            Delete,
            Semicolon,
            Equals,
            Numpad0,
            Numpad1,
            Numpad2,
            Numpad3,
            Numpad4,
            Numpad5,
            Numpad6,
            Numpad7,
            Numpad8,
            Numpad9,
            Multiply,
            Add,
            Separator,
            Subtract,
            Decimal,
            Divide,
            F1,
            F2,
            F3,
            F4,
            F5,
            F6,
            F7,
            F8,
            F9,
            F10,
            F11,
            F12,
            Meta,
            Command,
            Zenkaku_hankaku,
            Wait
        }
    }
}
