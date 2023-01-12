﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace MccSoft.TemplateApp.Domain.Audit;

public class AuditLog
{
    public int Id { get; set; }

    public DateTime ChangeDate { get; set; }

    /// <summary>
    /// Type of the changed entity
    /// </summary>
    public string EntityType { get; set; } = "";

    /// <summary>
    /// Insert/Update/Delete
    /// </summary>
    public string Action { get; set; } = "";

    /// <summary>
    /// If Key is a single column, will contain the value of this column
    /// Useful to easier SELECT objects based on their key.
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// Id of a User who performed the changes
    /// </summary>
    public string? UserId { get; set; }
    public User? User { get; set; }

    [Column(TypeName = "jsonb")]
    public JsonDocument FullKey { get; set; } = null!;

    [Column(TypeName = "jsonb")]
    public JsonDocument? Actual { get; set; }

    [Column(TypeName = "jsonb")]
    public JsonDocument? Change { get; set; }

    [Column(TypeName = "jsonb")]
    public JsonDocument? Old { get; set; }
}
