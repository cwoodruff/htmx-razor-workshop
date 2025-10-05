using System;

namespace htmx_examples_blazor;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
