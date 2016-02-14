﻿using UnityEngine;
using System.Collections;

public class BasicTextureVegetation : Vegetation
{
	public float slopeValue;
	public float mountainPeekHeight;
	private int width;
	private int height;
	private float[,] heightMap;
	private float[,,] map;
	private Terrain terrain;

	public override float[,,] Texture(int width, int height, float[,] heightMap, float[,,] map) {
		this.width = width;
		this.height = height;
		this.heightMap = heightMap;
		this.map = map;

		SlopeAndHeightTexture();

		return this.map;
	}
								
	private void SlopeAndHeightTexture()
	{
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {

				// read the height at this location
				float locationHeight = heightMap[y, x];

				// used for slope texturing
				var maxDifference = 0.0f;

				// Use a full Moore neighborhood.  A Von Neumann is faster but can result
				// in weird artifacts sometimes.
				for (int dx = -1; dx <= 1; dx += 1) {
					for (int dy = -1; dy <= 1; dy += 1) {
						int nx = x + dx;
						int ny = y + dy;

						if (nx < 0 || ny < 0 || nx >= width || ny >= height || (nx == 0 && ny == 0)) {
							// Skip neighbor cells that fall off the map.
							continue;
						}

						var temp = heightMap[y, x] - heightMap[ny, nx];

						if (temp > maxDifference) {
							maxDifference = temp;
						}
					}
				}

				if (locationHeight < waterHeight) {
					map[y, x, 0] = 0;
					map[y, x, 1] = 0;
					map[y, x, 2] = locationHeight;
					map[y, x, 3] = 0;
					map[y, x, 4] = 1 - locationHeight;
				} else if (locationHeight >= (mountainPeekHeight + Random.Range(0f, 0.4f))) { // mountain tops texturing
					var halfDiff = (1 - mountainPeekHeight) / 2;
					var quarterDiff = halfDiff / 2;
					if (locationHeight >= (mountainPeekHeight + halfDiff + quarterDiff)) { // highest peek
						map[y, x, 0] = 0;
						map[y, x, 1] = 0;
						map[y, x, 2] = 1 - locationHeight;
						map[y, x, 3] = locationHeight;
						map[y, x, 4] = 0;
					} else if (locationHeight <= (mountainPeekHeight + halfDiff)) { // lowest peek
						map[y, x, 0] = 0;
						map[y, x, 1] = locationHeight * 0.05f;
						map[y, x, 2] = locationHeight * 0.9f;
						map[y, x, 3] = 1 - locationHeight * 0.85f;
						map[y, x, 4] = 0;
					} else { // middle peek
						map[y, x, 0] = 0;
						map[y, x, 1] = 0;
						map[y, x, 2] = 1 - locationHeight * 0.5f;
						map[y, x, 3] = locationHeight * 0.5f;
						map[y, x, 4] = 0;
					}
				} else if (maxDifference > slopeValue) { // slope texturing
					map[y, x, 0] = 0;
					map[y, x, 1] = 0.15f;
					map[y, x, 2] = 0.84f;
					map[y, x, 3] = 0.01f;
					map[y, x, 4] = 0;
				} else { // default texturing based on height
					map[y, x, 0] = 2*(1 - locationHeight)/3;
					map[y, x, 1] = (1 - locationHeight)/3;
					map[y, x, 2] = locationHeight;
					map[y, x, 3] = 0;
					map[y, x, 4] = 0;
				}
			}
		}
	}

	public override void AddTrees(int width, int height, float[,] heightMap, Terrain terrain) {
		this.width = width;
		this.height = height;
		this.heightMap = heightMap;
		this.terrain = terrain;

		PopulateTreeInstances();
	}

	private void PopulateTreeInstances()
	{
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {

				// read the height at this location
				float locationHeight = heightMap [y, x];

				// used for slope texturing
				var maxDifference = 0.0f;

				// Use a full Moore neighborhood.  A Von Neumann is faster but can result
				// in weird artifacts sometimes.
				for (int dx = -1; dx <= 1; dx += 1) {
					for (int dy = -1; dy <= 1; dy += 1) {
						int nx = x + dx;
						int ny = y + dy;

						if (nx < 0 || ny < 0 || nx >= width || ny >= height || (nx == 0 && ny == 0)) {
							// Skip neighbor cells that fall off the map.
							continue;
						}

						var temp = heightMap [y, x] - heightMap [ny, nx];

						if (temp > maxDifference) {
							maxDifference = temp;
						}
					}
				}

				if (locationHeight < mountainPeekHeight && maxDifference < slopeValue && locationHeight > waterHeight) { // the slope and height is within limits
					
					var rnd = Random.Range(0, (600 + ((int) locationHeight * 100))); // fewer trees as it gets higher
					if (rnd == 0) {
						// add random trees	
						var treeInstance = new TreeInstance();
						treeInstance.prototypeIndex = Random.Range(0, 3); // mix of three tree prototypes
						treeInstance.heightScale = Random.Range(0.3f, 1.2f) - (0.3f * locationHeight); // smaller trees where height is more (less o2)
						treeInstance.widthScale = treeInstance.heightScale;
						treeInstance.color = Color.white;
						treeInstance.position = new Vector3((float) x / width, 0.0f, (float) y / height);

						terrain.AddTreeInstance(treeInstance);
					}
				}
			}
		}
	}
}

