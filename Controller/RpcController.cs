using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Nash_Manassas.Hub;
using Project_Manassas.Dto.Requests;

namespace Nash_Manassas.Controller;

[ApiController]
[Route("api/rpc")]
public class RpcController: ControllerBase
{
    private readonly ProjectApiClients _projectClient;

    public RpcController(ProjectApiClients projectClient)
    {
        _projectClient = projectClient;
    }

    [HttpPost]
    public async Task<IActionResult> HandleJsonRpc([FromBody] JsonRpcRequest req)
    {
        try
        {
            switch (req.Method)
            {
                case "list_projects":
                    return await HandleListProjects(req);
                    // var list = await _projectClient.ListProjectsAsync();
                    // return Ok(JsonRpcResponse.Ok(req.Id, list));
                
                case "find_project":
                    return await HandleFindProject(req);
                    // var name = req.Params?.GetProperty("projectName").GetString();
                    // var result = await _projectClient.FindProjectByNameAsync(name!);
                    // return Ok(JsonRpcResponse.Ok(req.Id, result));
                
                case "create_project":
                    return await HandleCreateProject(req);
                 // var request = req.Params?.Deserialize<CreateProjectRequest>();
                 // if (request == null)
                 //     return Ok(JsonRpcResponse.Error(req.Id, -32602, "Invalid params"));
                 //
                 // var created = await _projectClient.CreateProjectAsync(request);
                 // return Ok(JsonRpcResponse.Ok(req.Id, created));

                default:
                    return Ok(JsonRpcResponse.Error(req.Id, -32601, "Method not found"));
            }
        }
        catch (Exception err)
        {
            return new JsonResult(err.Message);
        }
    }
    
    private async Task<IActionResult> HandleFindProject(JsonRpcRequest req)
    {
        if (req.Params == null || req.Params.Value.ValueKind != JsonValueKind.Object)
            return Ok(JsonRpcResponse.Error(req.Id, -32602, "Missing or invalid params"));

        string? projectName = null;
        string? projectNumber = null;
        Guid? id = null;

        if (req.Params.Value.TryGetProperty("projectName", out var pn))
            projectName = pn.GetString();
        
        if (req.Params.Value.TryGetProperty("projectNumber", out var pnn))
            projectNumber = pnn.GetString();
        if (req.Params.Value.TryGetProperty("id", out var idp))
            id = Guid.Parse(idp.GetString());
        
        if (string.IsNullOrWhiteSpace(projectName))
            return Ok(JsonRpcResponse.Error(req.Id, -32602, "Missing required param: projectName"));

        var result = await _projectClient.FindProjectByNameAsync(projectName);
        
        return Ok(JsonRpcResponse.Ok(req.Id, result));
    }

    private async Task<IActionResult> HandleCreateProject(JsonRpcRequest req)
    {
        if (req.Params == null || req.Params.Value.ValueKind != JsonValueKind.Object)
        {
            return Ok(JsonRpcResponse.Error(req.Id, -32602, "Missing or invalid params"));
        }
        
        if (req == null) 
            return Ok(JsonRpcResponse.Error(req.Id, -32602, "Missing required param"));

        var request = req.Params.Value.Deserialize<CreateProjectRequest>();

        var created = await _projectClient.CreateProjectAsync(request);
        
        return Ok(JsonRpcResponse.Ok(req.Id, created));
    }
   
    private async Task<IActionResult> HandleListProjects(JsonRpcRequest req)
    {
        var projects = await _projectClient.ListProjectsAsync();
        return Ok(JsonRpcResponse.Ok(req.Id, projects));
    }
}

// --- JSON-RPC types ---
public class JsonRpcRequest
{
    public string Jsonrpc { get; set; } = "2.0";
    public string Id { get; set; } = "";
    public string Method { get; set; } = "";
    public JsonElement? Params { get; set; }
}

public static class JsonRpcResponse
{
    public static object Ok(string id, object result) =>
        new { jsonrpc = "2.0", id, result };

    public static object Error(string id, int code, string message) =>
        new { jsonrpc = "2.0", id, error = new { code, message } };
}