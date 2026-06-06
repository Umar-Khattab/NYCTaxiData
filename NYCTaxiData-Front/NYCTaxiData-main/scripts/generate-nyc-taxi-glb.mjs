/**
 * Generates a low-poly classic NYC sedan taxi GLB (untextured, white base mesh
 * so deck.gl ScenegraphLayer getColor applies NYC yellow #FDC500).
 */
import * as THREE from 'three'
import { GLTFExporter } from 'three/addons/exporters/GLTFExporter.js'
import fs from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

// Minimal polyfill for three.js GLTFExporter in Node
if (typeof globalThis.FileReader === 'undefined') {
  globalThis.FileReader = class FileReader {
    readAsArrayBuffer(blob) {
      blob.arrayBuffer().then((buf) => {
        this.result = buf
        this.onloadend?.({ target: this })
      })
    }
  }
}

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const outDir = path.join(__dirname, '..', 'public', 'models')
const outFile = path.join(outDir, 'nyc-yellow-taxi.glb')

const taxi = new THREE.Group()
taxi.name = 'NYCYellowTaxi'

const bodyMat = new THREE.MeshStandardMaterial({
  color: 0xffffff,
  metalness: 0.15,
  roughness: 0.55,
  flatShading: true,
})

const darkMat = new THREE.MeshStandardMaterial({
  color: 0x1a1a1a,
  metalness: 0.1,
  roughness: 0.85,
  flatShading: true,
})

const glassMat = new THREE.MeshStandardMaterial({
  color: 0x88aacc,
  metalness: 0.05,
  roughness: 0.15,
  transparent: true,
  opacity: 0.45,
  flatShading: true,
})

function box(w, h, d, mat, x, y, z) {
  const mesh = new THREE.Mesh(new THREE.BoxGeometry(w, h, d), mat)
  mesh.position.set(x, y, z)
  taxi.add(mesh)
  return mesh
}

// Forward = -Z (deck.gl scenegraph roll-90 convention used in fleet layer)
box(1.85, 0.42, 4.2, bodyMat, 0, 0.38, 0)
box(1.95, 0.72, 3.85, bodyMat, 0, 0.78, 0.05)
box(1.55, 0.95, 2.35, bodyMat, 0, 1.42, 0.15)
box(1.45, 0.08, 2.5, glassMat, 0, 1.52, -0.95)
box(0.55, 0.12, 1.1, bodyMat, 0, 1.92, 0.1)

const sign = box(1.05, 0.14, 0.55, bodyMat, 0, 2.02, 0.1)
sign.material = bodyMat.clone()

const wheelGeo = new THREE.CylinderGeometry(0.28, 0.28, 0.22, 10)
const wheelPositions = [
  [-0.95, 0.28, -1.25],
  [0.95, 0.28, -1.25],
  [-0.95, 0.28, 1.25],
  [0.95, 0.28, 1.25],
]
for (const [x, y, z] of wheelPositions) {
  const wheel = new THREE.Mesh(wheelGeo, darkMat)
  wheel.rotation.z = Math.PI / 2
  wheel.position.set(x, y, z)
  taxi.add(wheel)
}

box(0.12, 0.55, 0.08, darkMat, -0.98, 0.75, -1.95)
box(0.12, 0.55, 0.08, darkMat, 0.98, 0.75, -1.95)

taxi.rotation.y = 0

const exporter = new GLTFExporter()
exporter.parse(
  taxi,
  (gltf) => {
    fs.mkdirSync(outDir, { recursive: true })
    fs.writeFileSync(outFile, Buffer.from(gltf))
    console.log(`Wrote ${outFile} (${fs.statSync(outFile).size} bytes)`)
  },
  (err) => {
    console.error(err)
    process.exit(1)
  },
  { binary: true }
)
