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
                throw new Exception("Code " + key + " was not found.");
            }
        }

        /// <summary>
        /// Convert an element name into a code
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetElementCodeFromName(string name)
        {
            return UI8Mapping.First(n => n.Value.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Key;
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

            //Landmark types
            UI8Mapping.Add(80000, "Custom");
            UI8Mapping.Add(80001, "Form");
            UI8Mapping.Add(80002, "Main");
            UI8Mapping.Add(80003, "Navigation");
            UI8Mapping.Add(80004, "Search");
        }
    }
}