using Interop.UIAutomationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Edge.A11y
{
    /// <summary>
    /// This is used to wait for structure changed events
    /// </summary>
    public class StructureChangedHandler : IUIAutomationStructureChangedEventHandler
    {
        /// <summary>
        /// This is called when the event fires
        /// </summary>
        /// <param name="sender">The element in question</param>
        /// <param name="changeType">The type of change</param>
        void IUIAutomationStructureChangedEventHandler.HandleStructureChangedEvent(IUIAutomationElement sender, StructureChangeType changeType, int[] runtimeId)
        {
            if (changeType == StructureChangeType.StructureChangeType_ChildAdded)
            {
                TestData.Ewh.Set();
            }
        }
    }

    /// <summary>
    /// This is where the logic of the tests is stored
    /// </summary>
    class TestData
    {
        public const double epsilon = .001;

        /// <summary>
        /// The name of the test, which corresponds to the url of the page to test
        /// </summary>
        public string _TestName;
        /// <summary>
        /// The name of the UIA control type we will use to search for the element
        /// </summary>
        public string _ControlType;
        /// <summary>
        /// The name of the UIA localized control type, which will be part of the test
        /// case if it is not null
        /// </summary>
        public string _LocalizedControlType;
        /// <summary>
        /// The name of the UIA landmark type, which will be part of the test
        /// case if it is not null
        /// </summary>
        public string _LandmarkType;
        /// <summary>
        /// The name of the UIA localized landmark type, which will be part of the test
        /// case if it is not null
        /// </summary>
        public string _LocalizedLandmarkType;
        /// <summary>
        /// A list of ids for all the elements that should be keyboard accessible (via tab)
        /// </summary>
        public List<string> _KeyboardElements;
        /// <summary>
        /// If not null, this func will be used to test elements to see if they should be
        /// tested (instead of matching _ControlType).
        /// </summary>
        public Func<IUIAutomationElement, bool> _SearchStrategy;
        /// <summary>
        /// If not null, this will be appended to the repro path to create a full URL
        /// </summary>
        public string _RelativePath;
        /// <summary>
        /// A list of expected values which will be compared to the accessible names of
        /// the found elements.
        /// </summary>
        public List<string> _requiredNames;
        /// <summary>
        /// Same as above, but for accessible descriptions.
        /// </summary>
        public List<string> _requiredDescriptions;
        /// <summary>
        /// A func that allows extending the tests for specific elements. If an empty string is
        /// returned, the element passes. Otherwise, an explanation of its failure is returned.
        /// </summary>
        public Func<List<IUIAutomationElement>, DriverManager, List<string>, string> _AdditionalRequirement;

        /// <summary>
        /// Manual reset event waiter used to wait for elements to be added to UIA tree
        /// </summary>
        public static readonly EventWaitHandle Ewh = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// Simple Ctor
        /// </summary>
        /// <param name="testName"></param>
        /// <param name="controlType"></param>
        /// <param name="localizedControlType"></param>
        /// <param name="landmarkType"></param>
        /// <param name="localizedLandmarkType"></param>
        /// <param name="keyboardElements"></param>
        /// <param name="searchStrategy"></param>
        /// <param name="requiredNames"></param>
        /// <param name="requiredDescriptions"></param>
        /// <param name="additionalRequirement"></param>
        public TestData(string testName,
            string controlType,
            string localizedControlType = null,
            string landmarkType = null,
            string localizedLandmarkType = null,
            List<string> keyboardElements = null,
            Func<IUIAutomationElement, bool> searchStrategy = null,
            string relativePath = null,
            List<string> requiredNames = null,
            List<string> requiredDescriptions = null,
            Func<List<IUIAutomationElement>, DriverManager, List<string>, string> additionalRequirement = null)
        {
            _TestName = testName;
            _ControlType = controlType;
            _LocalizedControlType = localizedControlType;
            _LandmarkType = landmarkType;
            _LocalizedLandmarkType = localizedLandmarkType;
            _KeyboardElements = keyboardElements;
            _SearchStrategy = searchStrategy;
            _RelativePath = relativePath;
            _requiredNames = requiredNames;
            _requiredDescriptions = requiredDescriptions;
            _AdditionalRequirement = additionalRequirement;
        }

        //All the tests to run
        public static Lazy<List<TestData>> alltests = new Lazy<List<TestData>>(AllTests);

        /// <summary>
        /// Get the TestData for the given test page
        /// </summary>
        /// <param name="testName">The name of the file being tested</param>
        /// <returns>TestData for the given test page, or null if it couldn't be found</returns>
        public static TestData DataFromName(string testName)
        {
            return alltests.Value.FirstOrDefault(t => t._TestName == testName);
        }

        /// <summary>
        /// Singleton initializer
        /// </summary>
        /// <returns></returns>
        static List<TestData> AllTests()
        {
            return new List<TestData>{
                new TestData("Search Button", "Button", relativePath: "/")
            };
        }
    }
}
