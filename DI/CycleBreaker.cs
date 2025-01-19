using HanneBogaerts.GlueBox.Exceptions;
using QuikGraph;
using QuikGraph.Algorithms;
using Serilog;

namespace HanneBogaerts.GlueBox.DI;

public class CycleBreaker(AdhesionRegistry adhesionRegistry, AdhesionManager adhesionManager)
{
    public void DetectStickyLoop()
    {
        Log.Debug("CycleBreaker is checking for cyclic dependencies...");
        var dependencyGraph = BuildDependencyGraph();

        PrintGraph(dependencyGraph);

        if (dependencyGraph.IsDirectedAcyclicGraph())
        {
            Log.Information("No cycles detected in the dependency graph.");
        }
        else
        {
            Log.Error("A cycle has been detected in the dependency graph.");
            throw new StickyLoopException("A cycle has been detected in the dependency graph.");
        }
    }

    private BidirectionalGraph<Type, Edge<Type>> BuildDependencyGraph()
    {
        Log.Debug("Building the dependency graph from the gluebox registrations");
        var dependencyGraph = new BidirectionalGraph<Type, Edge<Type>>();
        foreach (var (service, glueBinding) in adhesionRegistry.GetRegistrations())
        {
            var implementation = glueBinding.ImplementationType;
            if (!dependencyGraph.ContainsVertex(service))
            {
                Log.Verbose("Adding vertex for service: {ServiceType}", service.Name);
                dependencyGraph.AddVertex(service);
            }

            if (service != implementation)
            {
                if (!dependencyGraph.ContainsVertex(implementation))
                {
                    Log.Verbose("Adding vertex for implementation: {ImplementationType}", implementation.Name);
                    dependencyGraph.AddVertex(implementation);
                }

                Log.Debug("Adding edge from {ServiceType} to {ImplementationType}", service.Name, implementation.Name);
                dependencyGraph.AddEdge(new Edge<Type>(service, implementation));
            }

            var dependencies = adhesionManager.GetDependenciesForService(implementation);

            foreach (var dependency in dependencies)
            {
                if (!dependencyGraph.ContainsVertex(dependency))
                {
                    Log.Verbose("Adding vertex for dependency: {DependencyType}", dependency.Name);
                    dependencyGraph.AddVertex(dependency);
                }

                Log.Debug("Adding edge from {ImplementationType} to {DependencyType}", implementation.Name,
                    dependency.Name);
                dependencyGraph.AddEdge(new Edge<Type>(implementation, dependency));
            }
        }

        Log.Debug("Finished building the dependency graph with {VertexCount} vertices and {EdgeCount} edges.",
            dependencyGraph.VertexCount, dependencyGraph.EdgeCount);
        return dependencyGraph;
    }

    private static void PrintGraph(BidirectionalGraph<Type, Edge<Type>> graph)
    {
        Log.Debug("Dependency Graph:");

        foreach (var vertex in graph.Vertices)
        {
            Log.Debug("Vertex: {VertexName}", vertex.Name);
        }

        foreach (var edge in graph.Edges)
        {
            Log.Debug("Edge: {Source} -> {Target}", edge.Source.Name, edge.Target.Name);
        }
    }
}