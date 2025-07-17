// Program.cs

using Microsoft.OpenApi.Models;
using System;
using InfoTask.Models;

var builder = WebApplication.CreateBuilder(args);

// Register the services needed to generate and expose Swagger/OpenAPI docs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Workflow API", Version = "v1" });
});

var app = builder.Build();

// Enable middleware to serve the Swagger JSON and UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workflow API V1");
});

// In-memory data stores to hold workflow-related data during the app's lifetime
var definitions = new Dictionary<string, WorkflowDefinition>();
var instances = new Dictionary<string, WorkflowInstance>();
var globalStates = new Dictionary<string, State>();
var globalActions = new Dictionary<string, WorkflowAction>();

// Endpoint to create a new global state object
app.MapPost("/states/create", (string id, string name, bool isFinal, bool isInitial, bool enabled) =>
{
    if (globalStates.ContainsKey(id))
        return Results.BadRequest("State ID already exists.");

    var state = new State
    {
        Id = id,
        Name = name,
        IsFinal = isFinal,
        IsInitial = isInitial,
        Enabled = enabled
    };

    globalStates[id] = state;
    return Results.Ok(state);
});

// Endpoint to define a global action between states
app.MapPost("/actions/create", (string id, string name, List<string> fromStates, string toState, bool enabled) =>
{
    if (globalActions.ContainsKey(id))
        return Results.BadRequest("Action ID already exists.");

    // Ensure all source states exist and are not final
    foreach (var fromStateId in fromStates)
    {
        if (!globalStates.TryGetValue(fromStateId, out var state))
            return Results.BadRequest($"FromState ID {fromStateId} not found.");

        if (state.IsFinal)
            return Results.BadRequest($"FromState '{fromStateId}' is a final state and cannot be used as a source.");
    }

    // Ensure the target state exists
    if (!globalStates.ContainsKey(toState))
        return Results.BadRequest($"ToState ID {toState} not found.");

    var action = new WorkflowAction
    {
        Id = id,
        Name = name,
        FromStates = fromStates,
        ToState = toState,
        Enabled = enabled
    };

    globalActions[id] = action;
    return Results.Ok(action);
});

// Add an existing global state to a specific workflow definition
app.MapPost("/workflowdef/{defId}/states/add", (string defId, string stateId) =>
{
    if (!definitions.TryGetValue(defId, out var def))
        return Results.NotFound("Workflow definition not found.");

    if (!globalStates.TryGetValue(stateId, out var state))
        return Results.BadRequest("State ID not found in global state list.");

    // Avoid duplicates
    if (def.States.Any(s => s.Id == state.Id))
        return Results.BadRequest("This state already exists in the workflow.");

    // Ensure there's only one initial state per workflow
    if (state.IsInitial && def.States.Any(s => s.IsInitial))
        return Results.BadRequest("A workflow can only have one initial state.");

    def.States.Add(state);
    return Results.Ok(state);
});

// Add a global action to a workflow definition
app.MapPost("/workflowdef/{defId}/actions/add", (string defId, string actionId) =>
{
    if (!definitions.TryGetValue(defId, out var def))
        return Results.NotFound("Workflow definition not found.");

    if (!globalActions.TryGetValue(actionId, out var actionToAdd))
        return Results.BadRequest($"Action with ID '{actionId}' not found.");

    if (def.Actions.Any(a => a.Id == actionToAdd.Id))
        return Results.BadRequest("This action already exists in the workflow.");

    // Make sure all states involved in the action exist in the workflow
    if (!def.States.Any(s => s.Id == actionToAdd.ToState))
        return Results.BadRequest("The action's 'ToState' must exist in the workflow.");

    foreach (var fromStateId in actionToAdd.FromStates)
    {
        if (!def.States.Any(s => s.Id == fromStateId))
            return Results.BadRequest($"The action's 'FromState' with ID '{fromStateId}' does not exist in the workflow.");
    }

    def.Actions.Add(actionToAdd);
    return Results.Ok(actionToAdd);
});

// Create a new, empty workflow definition
app.MapPost("/workflowdef", (string id, string description) =>
{
    if (definitions.ContainsKey(id))
        return Results.BadRequest("Duplicate workflow ID.");

    var def = new WorkflowDefinition { Id = id, Description = description };
    definitions[id] = def;
    return Results.Ok(def);
});

// Get details of a workflow definition by ID
app.MapGet("/workflowdef/{id}", (string id) =>
  definitions.TryGetValue(id, out var def)
    ? Results.Ok(def)
    : Results.NotFound("Definition not found.")
);

// Start a new instance of a workflow
app.MapPost("/instances/create", (string defId, string instanceId) =>
{
    if (!definitions.TryGetValue(defId, out var def))
        return Results.NotFound("Definition not found.");

    if (instances.ContainsKey(instanceId))
        return Results.BadRequest("Instance ID already exists.");

    // Workflow must have a valid, enabled initial state
    var initial = def.States.FirstOrDefault(s => s.IsInitial && s.Enabled);
    if (initial is null)
        return Results.BadRequest("Initial state missing or disabled.");

    var instance = new WorkflowInstance
    {
        Id = instanceId,
        DefinitionId = defId,
        CurrentStateId = initial.Id
    };

    instances[instance.Id] = instance;
    return Results.Ok(instance);
});

// Perform a transition (action) on a workflow instance
app.MapPost("/instances/{id}/actions/{actionId}", (string id, string actionId) =>
{
    if (!instances.TryGetValue(id, out var instance))
        return Results.NotFound("Instance not found.");

    if (!definitions.TryGetValue(instance.DefinitionId, out var def))
        return Results.NotFound("Workflow definition missing.");

    var action = def.Actions.FirstOrDefault(a => a.Id == actionId);
    if (action == null || !action.Enabled)
        return Results.BadRequest("Invalid or disabled action.");

    if (!action.FromStates.Contains(instance.CurrentStateId))
        return Results.BadRequest("Action not valid from current state.");

    var toState = def.States.FirstOrDefault(s => s.Id == action.ToState);
    if (toState == null || !toState.Enabled)
        return Results.BadRequest("Target state is invalid or disabled.");

    // Prevent transitions from a final state
    if (def.States.First(s => s.Id == instance.CurrentStateId).IsFinal)
        return Results.BadRequest("Cannot act on a final state.");

    instance.CurrentStateId = action.ToState;
    instance.History.Add(new WorkflowHistoryEntry(action.Id, DateTime.UtcNow));

    return Results.Ok(instance);
});

// Get the current state and history of a workflow instance
app.MapGet("/instances/{id}", (string id) =>
{
    if (!instances.TryGetValue(id, out var instance))
        return Results.NotFound("Instance not found.");

    return Results.Ok(instance);
});

app.Run();
