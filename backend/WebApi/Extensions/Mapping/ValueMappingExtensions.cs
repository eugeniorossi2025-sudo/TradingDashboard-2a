// WebApi/Extensions/Mapping/ValueMappingExtensions.cs

using Contracts.Value;
using Entities;

namespace WebApi.Extensions.Mapping;

/// <summary>
/// Extension methods for mapping Value entities.
/// </summary>
public static class ValueMappingExtensions
{
    /// <summary>
    /// Maps a CreateValueRequest to a Values entity.
    /// </summary>
    public static Values MapToEntity(this CreateValueRequest request)
    {
        return new Values
        {
            Key = request.Key,
            Description = request.Description,
            Value = request.Value,
            IdUser = request.IdUser,
            DateTime = DateTime.Now,
            Account = request.Account,
            Tavolo = request.Tavolo,
            Mazzo = request.Mazzo,
            Margine = request.Margine,
            MediaOra = request.MediaOra,
            Stato = request.Stato,
            Colore = request.Colore,
            ColpoMartingala = request.ColpoMartingala,
            Valutazione = request.Valutazione,
            Reason = request.Reason,
            Prediction = request.Prediction,
            Pbt = request.Pbt,
            Tempo = request.Tempo
        };
    }

    /// <summary>
    /// Updates an existing Values entity from UpdateValueRequest.
    /// </summary>
    public static void UpdateFromRequest(this Values value, UpdateValueRequest request)
    {
        if (request.Description != null) value.Description = request.Description;
        if (request.Value != null) value.Value = request.Value;
        if (request.Account != null) value.Account = request.Account;
        if (request.Tavolo.HasValue) value.Tavolo = request.Tavolo;
        if (request.Mazzo.HasValue) value.Mazzo = request.Mazzo;
        if (request.Margine.HasValue) value.Margine = request.Margine;
        if (request.MediaOra.HasValue) value.MediaOra = request.MediaOra;
        if (request.Stato != null) value.Stato = request.Stato;
        if (request.Colore != null) value.Colore = request.Colore;
        if (request.ColpoMartingala.HasValue) value.ColpoMartingala = request.ColpoMartingala;
        if (request.Valutazione != null) value.Valutazione = request.Valutazione;
        if (request.Reason != null) value.Reason = request.Reason;
        if (request.Prediction != null) value.Prediction = request.Prediction;
        if (request.Pbt != null) value.Pbt = request.Pbt;
        if (request.Tempo != null) value.Tempo = request.Tempo;
    }

    /// <summary>
    /// Maps a Values entity to ValueResponse.
    /// </summary>
    public static ValueResponse ToContract(this Values value)
    {
        return new ValueResponse
        {
            Id = value.Id,
            Key = value.Key,
            Description = value.Description,
            Value = value.Value,
            IdUser = value.IdUser,
            DateTime = value.DateTime,
            Account = value.Account,
            Tavolo = value.Tavolo,
            Mazzo = value.Mazzo,
            Margine = value.Margine,
            MediaOra = value.MediaOra,
            Stato = value.Stato,
            Colore = value.Colore,
            ColpoMartingala = value.ColpoMartingala,
            Valutazione = value.Valutazione,
            Reason = value.Reason,
            Prediction = value.Prediction,
            Pbt = value.Pbt,
            Tempo = value.Tempo
        };
    }
}