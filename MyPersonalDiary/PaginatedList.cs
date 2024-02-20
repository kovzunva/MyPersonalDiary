﻿using Elfie.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; private set; }
    public int TotalPages { get; private set; }
    public string Route { get; private set; }

    public PaginatedList(IQueryable<T> source, int pageIndex, int pageSize, string route)
    {
        TotalPages = (int)Math.Ceiling(source.Count() / (double)pageSize);
        Route = route;
        if (pageIndex < 1)
        {
            PageIndex = 1;
        }
        else if (pageIndex > TotalPages)
        {
            PageIndex = TotalPages;
        }
        else
        {
            PageIndex = pageIndex;
        }

        this.AddRange(source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList());
    }

    public bool HasPreviousPage
    {
        get { return (PageIndex > 1); }
    }

    public bool HasNextPage
    {
        get { return (PageIndex < TotalPages); }
    }

    public string GetPaginationString(int linksToLeftAndRight = 2)
    {
        var paginationString = "";

        if (TotalPages <= 1)
        {
            return paginationString;
        }

        // Previous link
        if (HasPreviousPage)
        {
            paginationString += $"<a href=\"{Route}?page={PageIndex - 1}\"> &lt; </a> ";
        }

        // First page link
        if (PageIndex - linksToLeftAndRight > 1)
        {
            paginationString += $"<a href=\"{Route}?page=1\">1</a> ";
            if (PageIndex - linksToLeftAndRight > 2)
            {
                paginationString += "<span>...</span>";
            }
        }

        // Page links
        for (int i = Math.Max(1, PageIndex - linksToLeftAndRight); i <= Math.Min(TotalPages, PageIndex + linksToLeftAndRight); i++)
        {
            if (i == PageIndex)
            {
                paginationString += $"<span>{i}</span> ";
            }
            else
            {
                paginationString += $"<a href=\"{Route}?page={i}\">{i}</a> ";
            }
        }

        // Last page link
        if (PageIndex + linksToLeftAndRight < TotalPages)
        {
            if (PageIndex + linksToLeftAndRight < TotalPages - 1)
            {
                paginationString += "<span>...</span>";
            }
            paginationString += $"<a href=\"{Route}?page={TotalPages}\">{TotalPages}</a> ";
        }

        // Next link
        if (HasNextPage)
        {
            paginationString += $"<a href=\"{Route}?page={PageIndex + 1}\"> &gt; </a>";
        }

        return paginationString;
    }
}
