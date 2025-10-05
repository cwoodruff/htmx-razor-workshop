using System;

namespace htmx_examples_blazor;

public interface IEndpoints
{
    void MapEndpoints(IEndpointRouteBuilder app);
}
