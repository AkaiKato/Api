﻿using Api.Models.Base;

namespace Api.Models
{
    public class User_group : BaseInfo
    {
        public string Code { get; set; } = null!;

        public string? Description { get; set; }
    }
}
