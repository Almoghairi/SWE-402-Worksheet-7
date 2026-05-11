# SWE-402 Worksheet 7 - Product Showcase Scene

Theme: Neon museum after midnight.

Open Assets/Scenes/ProductShowcase.unity. The scene contains grouped Products, Plinths, Lights, and Probes objects.

Checklist coverage:
- Five product objects on plinths with five distinct URP/Lit materials: matte, shiny non-metal, metal, transparent glass, and textured/emissive circuit material.
- CircuitTile.png is assigned as a base map with adjusted tiling. RidgedNormal.png is imported as a normal map and assigned to the textured material.
- Assets/Shaders/NeonCircuitShimmer.shadergraph is included and the ShaderGraph_Neon_Shimmer material is applied to Product_5.
- Directional, point, spot, and baked area lights are configured for a cool neon gallery mood, with soft shadows enabled.
- Ground and plinths are marked static. A LightProbeGroup and ReflectionProbe are placed near reflective/transparent products.
