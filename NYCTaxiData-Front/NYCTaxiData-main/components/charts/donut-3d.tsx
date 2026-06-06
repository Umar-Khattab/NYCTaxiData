'use client'

import { useEffect, useRef, useState } from 'react'
import * as THREE from 'three'

interface DonutSegment {
  name: string
  value: number // percentage
  color: string
}

interface Donut3dProps {
  onHoverSegment?: (name: string | null) => void
  hoveredSegmentName?: string | null
}

const DATA: DonutSegment[] = [
  { name: 'Manhattan', value: 58, color: '#1D4ED8' },
  { name: 'Brooklyn', value: 22, color: '#06B6D4' },
  { name: 'Queens', value: 12, color: '#0D9488' },
  { name: 'Bronx', value: 6, color: '#FB923C' },
  { name: 'Staten Island', value: 2, color: '#6B7280' },
]

export function Donut3d({ onHoverSegment, hoveredSegmentName }: Donut3dProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const [internalHover, setInternalHover] = useState<string | null>(null)

  useEffect(() => {
    if (!containerRef.current || !canvasRef.current) return

    const container = containerRef.current
    const canvas = canvasRef.current

    // Dimensions
    let width = container.clientWidth
    let height = container.clientHeight || 260

    // Scene, Camera, Renderer
    const scene = new THREE.Scene()
    scene.background = null

    const camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 100)
    camera.position.set(0, 0, 10)

    const renderer = new THREE.WebGLRenderer({
      canvas,
      antialias: true,
      alpha: true,
    })
    renderer.setSize(width, height)
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2))

    // Group to hold all segments
    const chartGroup = new THREE.Group()
    chartGroup.rotation.x = -Math.PI / 3.5
    scene.add(chartGroup)

    // Lights
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.8)
    scene.add(ambientLight)

    const dirLight = new THREE.DirectionalLight(0xffffff, 1.5)
    dirLight.position.set(5, 8, 10)
    scene.add(dirLight)

    const pointLight = new THREE.PointLight(0xa78bfa, 1.2, 30)
    pointLight.position.set(-5, -5, 5)
    scene.add(pointLight)

    // Build segments
    const innerRadius = 2.0
    const outerRadius = 3.2
    const extrudeSettings = {
      depth: 0.6,
      bevelEnabled: true,
      bevelSegments: 4,
      steps: 1,
      bevelSize: 0.06,
      bevelThickness: 0.06,
    }

    // Map segments to Meshes
    const segmentMeshes: { mesh: THREE.Mesh; data: DonutSegment; originalZ: number }[] = []

    let currentAngle = 0
    DATA.forEach((seg) => {
      const angleSweep = (seg.value / 100) * Math.PI * 2
      const startAngle = currentAngle
      const endAngle = currentAngle + angleSweep
      currentAngle = endAngle

      // Create 2D shape
      const shape = new THREE.Shape()
      shape.absarc(0, 0, outerRadius, startAngle, endAngle, false)
      shape.absarc(0, 0, innerRadius, endAngle, startAngle, true)

      // Extrude into 3D
      const geometry = new THREE.ExtrudeGeometry(shape, extrudeSettings)
      geometry.center()

      // High-end reflective/glowing material
      const material = new THREE.MeshStandardMaterial({
        color: new THREE.Color(seg.color),
        roughness: 0.15,
        metalness: 0.85,
        emissive: new THREE.Color(seg.color),
        emissiveIntensity: 0.15,
      })

      const mesh = new THREE.Mesh(geometry, material)
      
      const midAngle = startAngle + angleSweep / 2
      const liftDirection = new THREE.Vector3(Math.cos(midAngle), Math.sin(midAngle), 0).normalize()
      
      mesh.userData = {
        name: seg.name,
        color: seg.color,
        midAngle,
        liftDirection,
      }

      chartGroup.add(mesh)
      segmentMeshes.push({
        mesh,
        data: seg,
        originalZ: mesh.position.z,
      })
    })

    // Mouse Interaction
    const raycaster = new THREE.Raycaster()
    const mouse = new THREE.Vector2()
    let hoveredMesh: THREE.Mesh | null = null

    const onMouseMove = (event: MouseEvent) => {
      const rect = canvas.getBoundingClientRect()
      mouse.x = ((event.clientX - rect.left) / width) * 2 - 1
      mouse.y = -((event.clientY - rect.top) / height) * 2 + 1

      raycaster.setFromCamera(mouse, camera)
      const intersects = raycaster.intersectObjects(chartGroup.children)

      if (intersects.length > 0) {
        const firstIntersect = intersects[0].object as THREE.Mesh
        if (hoveredMesh !== firstIntersect) {
          resetHoverState()

          hoveredMesh = firstIntersect
          const name = hoveredMesh.userData.name
          setInternalHover(name)
          if (onHoverSegment) onHoverSegment(name)

          const liftDir = hoveredMesh.userData.liftDirection as THREE.Vector3
          const liftAmount = 0.25
          
          hoveredMesh.position.copy(liftDir.clone().multiplyScalar(liftAmount))
          hoveredMesh.position.z = 0.15

          const mat = hoveredMesh.material as THREE.MeshStandardMaterial
          mat.emissiveIntensity = 0.6
          mat.metalness = 0.5
        }
      } else {
        if (hoveredMesh) {
          resetHoverState()
        }
      }
    }

    const resetHoverState = () => {
      if (hoveredMesh) {
        hoveredMesh.position.set(0, 0, 0)
        const mat = hoveredMesh.material as THREE.MeshStandardMaterial
        mat.emissiveIntensity = 0.15
        mat.metalness = 0.85
        hoveredMesh = null
        setInternalHover(null)
        if (onHoverSegment) onHoverSegment(null)
      }
    }

    const onMouseLeave = () => {
      resetHoverState()
    }

    canvas.addEventListener('mousemove', onMouseMove)
    canvas.addEventListener('mouseleave', onMouseLeave)

    // Animation Loop
    let animationFrameId: number
    const animate = () => {
      animationFrameId = requestAnimationFrame(animate)

      // Continuously rotate if not hovered
      if (!hoveredMesh) {
        chartGroup.rotation.z += 0.005
      }

      // Sync outer hovered segment highlights (if hovered from the list instead of map)
      segmentMeshes.forEach(({ mesh }) => {
        const isHoveredExternally = hoveredSegmentName && mesh.userData.name === hoveredSegmentName
        const isHoveredInternally = hoveredMesh && mesh.userData.name === hoveredMesh.userData.name
        
        if (isHoveredExternally && !isHoveredInternally) {
          const liftDir = mesh.userData.liftDirection as THREE.Vector3
          const liftAmount = 0.25
          mesh.position.copy(liftDir.clone().multiplyScalar(liftAmount))
          mesh.position.z = 0.15
          
          const mat = mesh.material as THREE.MeshStandardMaterial
          mat.emissiveIntensity = 0.6
        } else if (!isHoveredExternally && !isHoveredInternally) {
          mesh.position.set(0, 0, 0)
          const mat = mesh.material as THREE.MeshStandardMaterial
          mat.emissiveIntensity = 0.15
        }
      })

      renderer.render(scene, camera)
    }
    animate()

    const handleResize = () => {
      if (!containerRef.current) return
      width = container.clientWidth
      height = container.clientHeight || 260
      camera.aspect = width / height
      camera.updateProjectionMatrix()
      renderer.setSize(width, height)
    }
    window.addEventListener('resize', handleResize)

    return () => {
      cancelAnimationFrame(animationFrameId)
      canvas.removeEventListener('mousemove', onMouseMove)
      canvas.removeEventListener('mouseleave', onMouseLeave)
      window.removeEventListener('resize', handleResize)
      
      segmentMeshes.forEach(({ mesh }) => {
        mesh.geometry.dispose()
        if (Array.isArray(mesh.material)) {
          mesh.material.forEach((m) => m.dispose())
        } else {
          mesh.material.dispose()
        }
      })
      renderer.dispose()
    }
  }, [onHoverSegment, hoveredSegmentName])

  return (
    <div ref={containerRef} className="relative w-full h-[260px] flex items-center justify-center overflow-hidden">
      <canvas ref={canvasRef} className="cursor-pointer select-none" />
      <div className="absolute top-2 right-2 bg-black/40 backdrop-blur-md border border-white/10 rounded-md px-2 py-0.5 text-[9px] font-mono tracking-wider text-muted-foreground uppercase glow-cyan select-none pointer-events-none">
        Three.js Engine
      </div>
    </div>
  )
}
