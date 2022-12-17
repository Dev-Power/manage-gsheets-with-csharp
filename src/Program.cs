using System.CommandLine;
using ShoppingList;

var rootCommand = new RootCommand("Manage your shopping cart");

var printCartCommand = new Command("print", "Print the items in the cart");
printCartCommand.SetHandler(async () =>
{
    var gsheetsHelper = new GSheetsHelper();
    try
    {
        await gsheetsHelper.PrintCartItems();
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
    }
});
rootCommand.AddCommand(printCartCommand);

var itemNameOption = new Option<string>(
    new[] {"--item-name", "-n"},
    description: "The name of the item"
);
itemNameOption.IsRequired = true;

var quantityOption = new Option<decimal>(
    new[] {"--quantity", "-q"},
    description: "The quantity of the item"
);
quantityOption.IsRequired = true;

var addItemCommand = new Command("add", "Add an item to the cart")
{
    itemNameOption,
    quantityOption
};
addItemCommand.SetHandler(async (itemName, quantity) =>
{
    var gsheetsHelper = new GSheetsHelper();
    try
    {
        await gsheetsHelper.AddItem(itemName, quantity);
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
    }
}, itemNameOption, quantityOption);
rootCommand.AddCommand(addItemCommand);

var removeItemCommand = new Command("remove", "Remove an item from the cart")
{
    itemNameOption
};
removeItemCommand.SetHandler(async (itemName) =>
{
    var gsheetsHelper = new GSheetsHelper();
    try
    {
        await gsheetsHelper.RemoveItem(itemName);
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
    }
}, itemNameOption);
rootCommand.AddCommand(removeItemCommand);


return rootCommand.InvokeAsync(args).Result;
