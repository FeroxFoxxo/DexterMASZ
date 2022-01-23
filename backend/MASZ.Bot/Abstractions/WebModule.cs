﻿using MASZ.Bot.Models;
using Microsoft.AspNetCore.Builder;

namespace MASZ.Bot.Abstractions;

public abstract class WebModule : Module
{
	public virtual string[] AddAuthorizationPolicy()
	{
		return Array.Empty<string>();
	}

	public virtual void PostWebBuild(WebApplication application, AppSettings appSettings)
	{
	}
}
