using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WorkOrderApplication.API.Services; // Adjust namespace if needed

namespace WorkOrderApplication.API.Endpoints;

public static class MesEndpoints
{
    public static void MapMesEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/mo/{mo}", GetMoInfo);
        group.MapGet("/mo/list", GetMoList);
        group.MapGet("/line-info", GetLineInfo);
        group.MapGet("/sn/{sn}", GetSnInfo);
    }
    // GET_MO_INFO
    private static async Task<IResult> GetMoInfo(
        string mo,
        int moType,
        MesTdcClient mes)
    {
        var routingData = $"{moType}" + "}" + $"{mo}";

        var raw = await mes.CallAsync(
            testType: "GET_MO_INFO",
            routingData: routingData
        );

        var desc = raw.GetProperty("description");

        if (desc.ValueKind == JsonValueKind.String)
        {
            var text = desc.GetString();

            if (text == "{0}")
            {
                return Results.Ok(new
                {
                    mo,
                    targetQty = 0,
                    customers = Array.Empty<string>()
                });
            }

            return Results.Ok(new { message = text });
        }

        return Results.Ok(desc);
    }

    // MOList
    private static async Task<IResult> GetMoList(
        string empNo,
        string getDataType,
        string moType,
        string? line,
        MesQueryClient mes)
    {
        var raw = await mes.GetMoListAsync(
            empNo,
            getDataType,
            moType,
            line
        );

        var result = raw.GetProperty("Result").GetString();
        if (result != "OK")
        {
            return Results.BadRequest(
                raw.GetProperty("Message").GetString()
            );
        }

        return Results.Ok(
            raw.GetProperty("Message")
        );
    }

    // LineInfo
    private static async Task<IResult> GetLineInfo(
        string empNo,
        string? line,
        MesQueryClient mes)
    {
        var raw = await mes.GetLineInfoAsync(empNo, line);

        if (raw.GetProperty("Result").GetString() != "OK")
            return Results.BadRequest(
                raw.GetProperty("Message").GetString()
            );

        return Results.Ok(
            raw.GetProperty("Message")
        );
    }

    // SNInfo
    private static async Task<IResult> GetSnInfo(
        string sn,
        MesQueryClient mes)
    {
        var raw = await mes.GetSnInfoAsync(sn);

        if (raw.GetProperty("Result").GetString() != "OK")
            return Results.BadRequest(
                raw.GetProperty("Message").GetString()
            );

        return Results.Ok(
            raw.GetProperty("Message")
        );
    }
}
