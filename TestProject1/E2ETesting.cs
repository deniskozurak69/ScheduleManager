using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Playwright;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryWebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Spreadsheet;

[TestClass]
public class E2ETesting
{
    private IBrowser _browser;
    private IPage _page;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        _page = await _browser.NewPageAsync();
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        await _browser.CloseAsync();
    }

    [TestMethod]
    [Priority(1)]
    public async Task UserList_DisplayUsers()
    {
        await _page.GotoAsync("https://localhost:44379/Users");
        Console.WriteLine("Navigated to Users page");
        try
        {
            await _page.WaitForSelectorAsync(".table tbody tr", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 60000 });
            Console.WriteLine("User items are visible");
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine("Timeout waiting for user items: " + ex.Message);
            throw;
        }

        var userElements = await _page.QuerySelectorAllAsync(".table tbody tr");
        Console.WriteLine($"Found {userElements.Count} user items");
        Assert.IsTrue(userElements.Count > 0);
    }

    [TestMethod]
    [Priority(2)]
    public async Task CreateUser_NewUserAppearsInList()
    {
        await _page.GotoAsync("https://localhost:44379/Users/Create");
        Console.WriteLine("Navigated to Create User page");
        await _page.FillAsync("input[name='Username']", "TestUser");
        await _page.FillAsync("input[name='Password']", "TestPassword");
        await _page.FillAsync("input[name='Latitude']", "50.4501");
        await _page.FillAsync("input[name='Longtitude']", "30.5234");
        var fileInput = await _page.QuerySelectorAsync("input[name='photoFile']");
        await fileInput.SetInputFilesAsync("photo.jpg");
        await _page.ClickAsync("input[type='submit']");
        Console.WriteLine("Form submitted and navigated");
        await _page.GotoAsync("https://localhost:44379/Users");
        try
        {
            await _page.WaitForSelectorAsync("text=TestUser", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 60000 });
            Console.WriteLine("New user is visible");
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine("Timeout waiting for new user: " + ex.Message);
            throw;
        }
        var newUser = await _page.QuerySelectorAsync("text=TestUser");
        Assert.IsNotNull(newUser);
    }

    [TestMethod]
    [Priority(3)]
    public async Task EditUser()
    {
        await _page.GotoAsync("https://localhost:44379/Users");
        await _page.ClickAsync(".table tbody tr:has-text('TestUser') >> a:has-text('Edit')");
        Console.WriteLine("Navigated to Edit User page");
        await _page.FillAsync("input[name='Username']", "New username");
        await _page.FillAsync("input[name='Password']", "TestPassword");
        await _page.FillAsync("input[name='Latitude']", "50.4501");
        await _page.FillAsync("input[name='Longtitude']", "30.5234");
        var fileInput = await _page.QuerySelectorAsync("input[name='photoFile']");
        await fileInput.SetInputFilesAsync("photo.jpg");
        await _page.ClickAsync("input[type='submit']");
        Console.WriteLine("Form submitted and navigated");
        await _page.GotoAsync("https://localhost:44379/Users");
        try
        {
            await _page.WaitForSelectorAsync("text=New username", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 60000 });
            Console.WriteLine("Edited user is visible");
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine("Timeout waiting for edited user: " + ex.Message);
            throw;
        }
        var newUser = await _page.QuerySelectorAsync("text=New username");
        Assert.IsNotNull(newUser);
    }

    [TestMethod]
    [Priority(4)]
    public async Task UserDetails_DisplayCorrectInformation()
    {
        await _page.GotoAsync("https://localhost:44379/Users");
        try
        {
            await _page.WaitForSelectorAsync("text=New username", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 60000 });
            Console.WriteLine("Edited user is visible");
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine("Timeout waiting for edited user: " + ex.Message);
            throw;
        }
        await _page.ClickAsync(".table tbody tr:has-text('New username') >> a:has-text('Details')");
        await _page.WaitForSelectorAsync(".card", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 60000 });
        var username = (await _page.TextContentAsync(".card dd:nth-of-type(2)")).Trim();
        Assert.AreEqual("New username", username);
    }

    [TestMethod]
    [Priority(5)]
    public async Task DeleteUser()
    {
        await _page.GotoAsync("https://localhost:44379/Users");
        var userElements = await _page.QuerySelectorAllAsync(".table tbody tr");
        await _page.ClickAsync(".table tbody tr:has-text('New username') >> a:has-text('Delete')");
        Console.WriteLine("Navigated to Delete User page");
        await _page.ClickAsync("input[type='submit']");
        Console.WriteLine("User deleted");
        await _page.GotoAsync("https://localhost:44379/Users");
        var newUserElements = await _page.QuerySelectorAllAsync(".table tbody tr");
        Assert.IsTrue(userElements.Count > newUserElements.Count);
    }
}