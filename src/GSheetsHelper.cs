using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace ShoppingList;

public class GSheetsHelper
{
    private SheetsService _sheetsService;
    private string _spreadsheetId = "{ YOUR SPREADSHEET'S ID }";
    private string _range = "Cart!A2:B";
    
    public GSheetsHelper()
    {
        GoogleCredential credential;
        using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(SheetsService.Scope.Spreadsheets);
        }

        _sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "ShoppingList"
        });        
    }

    public async Task PrintCartItems()
    {
        SpreadsheetsResource.ValuesResource.GetRequest getRequest = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, _range);
       
        var getResponse = await getRequest.ExecuteAsync();
        IList<IList<Object>> values = getResponse.Values;
        if (values != null && values.Count > 0)
        {
            Console.WriteLine("Item\t\t\tQuantity");
            
            foreach (var row in values)
            {
                Console.WriteLine($"{row[0]}\t\t\t{row[1]}");
            }
        }
    }

    public async Task AddItem(string itemName, decimal quantity)
    {
        var valuesToInsert = new List<object>
        {
            itemName,
            quantity
        };

        SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum valueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
        SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum insertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

        var requestBody = new ValueRange();
        requestBody.Values = new List<IList<object>>();
        requestBody.Values.Add(valuesToInsert);

        SpreadsheetsResource.ValuesResource.AppendRequest appendRequest = _sheetsService.Spreadsheets.Values.Append(requestBody, _spreadsheetId, _range);
        appendRequest.ValueInputOption = valueInputOption;
        appendRequest.InsertDataOption = insertDataOption;
        
        await appendRequest.ExecuteAsync();
    }

    public async Task RemoveItem(string itemName)
    {
        SpreadsheetsResource.ValuesResource.GetRequest getRequest = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, _range);
        
        var getResponse = await getRequest.ExecuteAsync();
        IList<IList<Object>> values = getResponse.Values;
        if (values != null && values.Count > 0)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i][0].ToString() == itemName)
                {
                    var request = new Request
                    {
                        DeleteDimension = new DeleteDimensionRequest
                        {
                            Range = new DimensionRange
                            {
                                SheetId = 0,
                                Dimension = "ROWS",
                                StartIndex = i + 1,
                                EndIndex = i + 2
                            }
                        }
                    };
                    
                    var deleteRequest = new BatchUpdateSpreadsheetRequest {Requests = new List<Request> {request}};
                    var batchUpdateRequest = new SpreadsheetsResource.BatchUpdateRequest(_sheetsService, deleteRequest, _spreadsheetId);
                    await batchUpdateRequest.ExecuteAsync();
                }
            }
        }
    }
}