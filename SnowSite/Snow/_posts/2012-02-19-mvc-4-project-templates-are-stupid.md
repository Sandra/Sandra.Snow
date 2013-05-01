---
layout: post
title: MVC 4 Project Templates are stupid
category: Rant
---

*begin rant*

Thought I would jump on the MVC 4 Beta bandwagon today, installed, create new project... And this is what I'm greeted with:

    <?xml version="1.0" encoding="utf-8"?>
    <packages>
      <package id="AspNetMvc" version="4.0.20126.16343" />
      <package id="AspNetRazor.Core" version="2.0.20126.16343" />
      <package id="AspNetWebApi" version="4.0.20126.16343" />
      <package id="AspNetWebApi.Core" version="4.0.20126.16343" />
      <package id="AspNetWebPages.Core" version="2.0.20126.16343" />
      <package id="EntityFramework" version="4.1.10331.0" />
      <package id="jQuery" version="1.6.2" />
      <package id="jQuery.Ajax.Unobtrusive" version="2.0.20126.16343" />
      <package id="jQuery.UI.Combined" version="1.8.11" />
      <package id="jQuery.Validation" version="1.8.1" />
      <package id="jQuery.Validation.Unobtrusive" version="2.0.20126.16343" />
      <package id="knockoutjs" version="2.0.0.0" />
      <package id="Microsoft.Web.Infrastructure" version="1.0.0.0" />
      <package id="Microsoft.Web.Optimization" version="1.0.0-beta" />
      <package id="Modernizr" version="2.0.6" />
      <package id="System.Json" version="4.0.20126.16343" />
      <package id="System.Net.Http" version="2.0.20126.16343" />
      <package id="System.Net.Http.Formatting" version="4.0.20126.16343" />
      <package id="System.Web.Http.Common" version="4.0.20126.16343" />
      <package id="System.Web.Providers" version="1.1" />
      <package id="System.Web.Providers.Core" version="1.0" />
    </packages>

This is absolutely stupid...

I select EMPTY project template.

EMPTY

<!--excerpt-->

So I removed the things I don't need to begin with:

    <?xml version="1.0" encoding="utf-8"?>
    <packages>
      <package id="AspNetMvc" version="4.0.20126.16343" />
      <package id="AspNetRazor.Core" version="2.0.20126.16343" />
      <package id="AspNetWebApi" version="4.0.20126.16343" />
      <package id="AspNetWebApi.Core" version="4.0.20126.16343" />
      <package id="AspNetWebPages.Core" version="2.0.20126.16343" />
      <package id="Microsoft.Web.Infrastructure" version="1.0.0.0" />
      <package id="Microsoft.Web.Optimization" version="1.0.0-beta" />
      <package id="System.Json" version="4.0.20126.16343" />
      <package id="System.Net.Http" version="2.0.20126.16343" />
      <package id="System.Net.Http.Formatting" version="4.0.20126.16343" />
      <package id="System.Web.Http.Common" version="4.0.20126.16343" />
      <package id="System.Web.Providers" version="1.1" />
      <package id="System.Web.Providers.Core" version="1.0" />
    </packages>
    
But beyond that I don't know if the rest can be removed or if it's required by MVC 4. Time for some trial and error :)

I really hope MS decide to create a REAL empty project.