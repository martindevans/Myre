The GBuffer is split into 4 render targets:

## gbuffer_depth

 - R contains depth information

## gbuffer_depth_downsample

Half size copy of gbuffer_depth

 - R contains depth information

## gbuffer_normals

 - RG contains normal information
 - B contains specular intensity
 - A set to non-zero for all written pixels (can be used as a stencil)

## gbuffer_diffuse

 - RGB contains diffuse colour information
 - A contains specular power