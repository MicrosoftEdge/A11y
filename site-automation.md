# Automating your own site accessibility testing

One of the benefits of providing this tool via open source is that you
can you utilize the test harness to test any assistive technology that
utilizes UIA.

## Testing your site

Setting the root Domain
------------------------
1. Open up `Program.cs` in your IDE of choice
2. Change the value within `new EdgeStrategy("http://www.bing.com")` to whatever Url you want
   your root URL to be.
   
   Example: `new EdgeStrategy("http://localhost:8080")`

Adding the Tests
------------------------
1. Open up `TestData.cs`
2. Modify the `AllTests()` method by adding to the collection of
   typ `List<TestData>`.
3. For example, if we wanted to test that the "info" link is on the page and 
   in the accessiblity tree, we can add it by adding the following:

    new TestData("Info Link", "Hyperlink", relativePath: "/")

Understanding the TestData signature
-----------------------
The TestData class is where you enumerate what tests you want to run.

* **_TestName:** This is the name for either the test file (eg: article) or can be used as a description if your test name does not match that of the test files your testing.
* **_ControlType:** The name of the UIA control type we will use to search for the element.
* **_LocalizedControlType:** The name of the UIA localized control type
* **_LocalizedLandmarkType:** The name of the UIA localized landmark type
* **_KeyboardElements:** Automatically the system will tab through the site and create a collection of the `id` values of the elements that tab landed on.
* **_AdditionalRequirement:** This allows you to additional checks beyond the boilerplate of checking if it is in the accessibilty tree. Here you can call additional methods to check the computed name or description and more. Take a look at some of the examples on the [dev branch](https://github.com/MicrosoftEdge/A11y/blob/dev/TestData.cs).
* **_SearchStrategy:** If not null, this func will be used to test elements to see if they should be tested (instead of matching _ControlType).
* **_RelativePath:** This allows you to test subpages of your site by providing the relative path to the root domain (eg: `relativePath: "/aboutUs/"`)
