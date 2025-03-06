using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IoNote.DatabaseModels;

public partial class image
{
    [Key]
    public int imageid { get; set; }

    public string name { get; set; } = null!;

    public int noteid { get; set; }

    public byte[] data { get; set; } = null!;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime createdat { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? updatedat { get; set; }

    [ForeignKey("noteid")]
    [InverseProperty("images")]
    public virtual note note { get; set; } = null!;
}
