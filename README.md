# Final-Project-INFR-1350U

Chris Mugford:
- Toon Shading
- Normal & Bump Mapping
- Rim Lighting
- Water
- PBR

Owen Miller:
- Toon Shading
- Implementing Scene Objects
- Glass
- Handling Multiple UVs
- Transparency

# Progression
Explanation:
So far, we've implemented and shipped shaders for water and rim lighting in the build, and have more for toon lighting and PBR in the works.
The water shader is a heavily modified version of that which was demonstrated in one of the first tutorials, accounting for things like very low sine values, adjustable wave heights and slopes, as well as noise scaling and tiling parameters to produce more wave-like movements. For instance, the noise used to determine vertex height has functions for both standard tiling and dynamic scaling (both of which can be changed per-material), giving a greater impression of waves moving with the wind.

<img width="2911" height="1319" alt="image" src="https://github.com/user-attachments/assets/ee0b5cae-b56e-48c7-bd9a-a9da192fb85c" />
Additionally, we do plan to add foam around object edges (or some other Stencil Buffer and/or Z-Buffer related effect), so long as it works well with the low-poly visual style.

As for the rim lighting, we modified the rim color to be HDR, allowing for direct intensity control while leaving the fresnel effect power untouched. This will be especially important for fine-tuning it once we finalize our game project genre, and can be easily extended by, for instance, scaling said color value.

Progression Video:
[https://youtu.be/V-njmRiyM_s](url)

Reflection Document:
[https://docs.google.com/document/d/1yLqOTqad9ZAc6U67J6ddWtjOBF9d4Oyz-1ty_vcrWvc/edit?usp=sharing](url)

# Proposal
Link to proposal:
[https://docs.google.com/document/d/1m8I7wIQMOZIZPgxBiW34AmUMOmYRjqXG2g7gvlsrP-0/edit?usp=sharing](url)
