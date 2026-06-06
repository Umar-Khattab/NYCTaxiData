"use client";

import { useEffect, useRef } from "react";
import * as THREE from "three";

/* ─────────────────────────────────────────────────────────────
   Cyberpunk 3D Scene — Low-poly Taxi + Wave Grid + Particles
───────────────────────────────────────────────────────────── */
export function Cyber3DScene() {
  const mountRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const el = mountRef.current;
    if (!el) return;

    /* ── Renderer ── */
    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderer.setSize(el.clientWidth, el.clientHeight);
    renderer.setClearColor(0x000000, 0);
    el.appendChild(renderer.domElement);

    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(52, el.clientWidth / el.clientHeight, 0.1, 500);
    camera.position.set(0, 6, 18);
    camera.lookAt(0, 1, 0);

    scene.fog = new THREE.FogExp2(0x04020a, 0.032);

    /* ════════════════════════════════════════
       BUILD LOW-POLY TAXI CAR
       All parts as wireframe + edge highlights
    ════════════════════════════════════════ */
    const carGroup = new THREE.Group();
    scene.add(carGroup);

    const matWireViolet = new THREE.MeshBasicMaterial({
      color: 0x8b5cf6, wireframe: true, transparent: true, opacity: 0.55,
    });
    const matWireCyan = new THREE.MeshBasicMaterial({
      color: 0x06b6d4, wireframe: true, transparent: true, opacity: 0.65,
    });
    const matEdge = new THREE.LineBasicMaterial({
      color: 0xa78bfa, transparent: true, opacity: 0.9,
    });
    const matEdgeCyan = new THREE.LineBasicMaterial({
      color: 0x22d3ee, transparent: true, opacity: 0.9,
    });

    function addEdges(geo: THREE.BufferGeometry, mat: THREE.LineBasicMaterial, parent: THREE.Object3D) {
      parent.add(new THREE.LineSegments(new THREE.EdgesGeometry(geo), mat));
    }

    // ── Chassis (main body) ──
    const chassisGeo = new THREE.BoxGeometry(4.0, 0.55, 8.0, 2, 1, 3);
    const chassis = new THREE.Mesh(chassisGeo, matWireViolet);
    chassis.position.y = 0.62;
    carGroup.add(chassis);
    addEdges(chassisGeo, matEdge, chassis);

    // ── Lower body sides ──
    const bodyGeo = new THREE.BoxGeometry(4.4, 0.9, 7.4, 2, 1, 3);
    const body = new THREE.Mesh(bodyGeo, matWireViolet);
    body.position.y = 1.1;
    carGroup.add(body);
    addEdges(bodyGeo, matEdge, body);

    // ── Cabin (upper) ──
    const cabinGeo = new THREE.BoxGeometry(3.4, 1.1, 4.2, 1, 1, 2);
    const cabin = new THREE.Mesh(cabinGeo, matWireCyan);
    cabin.position.set(0, 2.1, 0.3);
    carGroup.add(cabin);
    addEdges(cabinGeo, matEdgeCyan, cabin);

    // ── Windshield (front angled panel) ──
    const wshieldPts = [
      new THREE.Vector3(-1.7, 0, 0),
      new THREE.Vector3(1.7, 0, 0),
      new THREE.Vector3(1.5, 1.0, -1.4),
      new THREE.Vector3(-1.5, 1.0, -1.4),
    ];
    const wshieldGeo = new THREE.BufferGeometry();
    wshieldGeo.setFromPoints(wshieldPts);
    const wshieldIdx = new Uint16Array([0, 1, 2, 0, 2, 3]);
    wshieldGeo.setIndex(new THREE.BufferAttribute(wshieldIdx, 1));
    wshieldGeo.computeVertexNormals();
    const wshield = new THREE.Mesh(
      wshieldGeo,
      new THREE.MeshBasicMaterial({ color: 0x06b6d4, transparent: true, opacity: 0.08, side: THREE.DoubleSide })
    );
    wshield.position.set(0, 1.55, -1.75);
    carGroup.add(wshield);
    // Windshield edge lines
    const wEdgePts = [...wshieldPts, wshieldPts[0]];
    const wEdgeGeo = new THREE.BufferGeometry().setFromPoints(wEdgePts);
    carGroup.add(new THREE.Line(wEdgeGeo, matEdgeCyan));

    // ── Rear window ──
    const rwinGeo = new THREE.BufferGeometry();
    const rwinPts = [
      new THREE.Vector3(-1.5, 1.0, 0),
      new THREE.Vector3(1.5, 1.0, 0),
      new THREE.Vector3(1.7, 0, 1.6),
      new THREE.Vector3(-1.7, 0, 1.6),
    ];
    rwinGeo.setFromPoints([...rwinPts, rwinPts[0]]);
    const rwin = new THREE.Line(rwinGeo, matEdgeCyan);
    rwin.position.set(0, 1.55, 1.65);
    carGroup.add(rwin);

    // ── Hood (front sloped) ──
    const hoodPts = [
      new THREE.Vector3(-2.1, 0, 0),
      new THREE.Vector3(2.1, 0, 0),
      new THREE.Vector3(1.9, 0.5, -2.2),
      new THREE.Vector3(-1.9, 0.5, -2.2),
    ];
    const hoodLineGeo = new THREE.BufferGeometry().setFromPoints([...hoodPts, hoodPts[0]]);
    const hood = new THREE.Line(hoodLineGeo, matEdge);
    hood.position.set(0, 1.08, -1.9);
    carGroup.add(hood);

    // ── Trunk (rear) ──
    const trunkPts = [
      new THREE.Vector3(-2.1, 0, 0),
      new THREE.Vector3(2.1, 0, 0),
      new THREE.Vector3(1.9, 0.45, 1.8),
      new THREE.Vector3(-1.9, 0.45, 1.8),
    ];
    const trunkLineGeo = new THREE.BufferGeometry().setFromPoints([...trunkPts, trunkPts[0]]);
    const trunk = new THREE.Line(trunkLineGeo, matEdge);
    trunk.position.set(0, 1.08, 1.7);
    carGroup.add(trunk);

    // ── Wheels (4x) ──
    const wheelGeo = new THREE.CylinderGeometry(0.7, 0.7, 0.45, 8);
    const wheelPositions = [
      [-2.15, 0.68,  2.4],
      [ 2.15, 0.68,  2.4],
      [-2.15, 0.68, -2.4],
      [ 2.15, 0.68, -2.4],
    ] as const;

    const wheelMeshes: THREE.Mesh[] = [];
    wheelPositions.forEach(([x, y, z]) => {
      const wm = new THREE.Mesh(wheelGeo, matWireCyan);
      wm.rotation.z = Math.PI / 2;
      wm.position.set(x, y, z);
      carGroup.add(wm);
      addEdges(wheelGeo, matEdgeCyan, wm);
      wheelMeshes.push(wm);

      // Wheel hub (inner disc)
      const hubGeo = new THREE.CylinderGeometry(0.3, 0.3, 0.5, 6);
      const hub = new THREE.Mesh(hubGeo, matWireViolet);
      hub.rotation.z = Math.PI / 2;
      hub.position.set(x, y, z);
      carGroup.add(hub);
    });

    // ── Headlights (front) ──
    [[-1.5, 1.2, -3.85], [1.5, 1.2, -3.85]].forEach(([x, y, z]) => {
      const hgeo = new THREE.SphereGeometry(0.22, 6, 6);
      const hm = new THREE.Mesh(hgeo, new THREE.MeshBasicMaterial({ color: 0x22d3ee, transparent: true, opacity: 0.8 }));
      hm.position.set(x, y, z);
      carGroup.add(hm);
      // Glow halo
      const haloGeo = new THREE.RingGeometry(0.25, 0.38, 8);
      const haloMat = new THREE.MeshBasicMaterial({ color: 0x06b6d4, side: THREE.DoubleSide, transparent: true, opacity: 0.35 });
      const halo = new THREE.Mesh(haloGeo, haloMat);
      halo.position.set(x, y, z - 0.1);
      carGroup.add(halo);
    });

    // ── Tail lights (rear) ──
    [[-1.5, 1.2, 3.85], [1.5, 1.2, 3.85]].forEach(([x, y, z]) => {
      const tgeo = new THREE.SphereGeometry(0.18, 6, 6);
      const tm = new THREE.Mesh(tgeo, new THREE.MeshBasicMaterial({ color: 0xf97316, transparent: true, opacity: 0.8 }));
      tm.position.set(x, y, z);
      carGroup.add(tm);
    });

    // ── Taxi light bar (roof) ──
    const barGeo = new THREE.BoxGeometry(1.0, 0.22, 1.0);
    const bar = new THREE.Mesh(barGeo, new THREE.MeshBasicMaterial({ color: 0xfbbf24, transparent: true, opacity: 0.9 }));
    bar.position.set(0, 2.72, 0.3);
    carGroup.add(bar);
    addEdges(barGeo, new THREE.LineBasicMaterial({ color: 0xfde68a, transparent: true, opacity: 1 }), bar);

    // ── Neon underflow (decorative lines) ──
    const underGeo = new THREE.BufferGeometry().setFromPoints([
      new THREE.Vector3(-2.1, 0, -3.8),
      new THREE.Vector3(-2.1, 0,  3.8),
    ]);
    const underMat = new THREE.LineBasicMaterial({ color: 0x7c3aed, transparent: true, opacity: 0.7 });
    [-2.1, 2.1].forEach((x) => {
      const g = new THREE.BufferGeometry().setFromPoints([
        new THREE.Vector3(x, 0.28, -3.8),
        new THREE.Vector3(x, 0.28, 3.8),
      ]);
      carGroup.add(new THREE.Line(g, underMat));
    });

    // Car initial position & rotation
    carGroup.position.set(0, -0.2, 0);
    carGroup.rotation.y = Math.PI * 0.15;

    /* ════════════════════════════════════════
       WAVE GRID (floor)
    ════════════════════════════════════════ */
    const COLS = 60, ROWS = 60, GSIZE = 36;
    const waveGeo = new THREE.PlaneGeometry(GSIZE, GSIZE, COLS - 1, ROWS - 1);
    waveGeo.rotateX(-Math.PI / 2);
    const posArr = waveGeo.attributes.position.array as Float32Array;
    const origY = new Float32Array(posArr.length / 3);
    for (let i = 0; i < posArr.length / 3; i++) origY[i] = posArr[i * 3 + 1];

    const waveCols = new Float32Array((posArr.length / 3) * 3);
    waveGeo.setAttribute("color", new THREE.BufferAttribute(waveCols, 3));
    const waveMesh = new THREE.Mesh(waveGeo, new THREE.MeshBasicMaterial({
      vertexColors: true, wireframe: true, transparent: true, opacity: 0.35,
    }));
    waveMesh.position.y = -2.2;
    scene.add(waveMesh);

    /* ════════════════════════════════════════
       PARTICLES (data rain)
    ════════════════════════════════════════ */
    const PCOUNT = 500;
    const pGeo = new THREE.BufferGeometry();
    const pPos = new Float32Array(PCOUNT * 3);
    const pVel = new Float32Array(PCOUNT);
    for (let i = 0; i < PCOUNT; i++) {
      pPos[i * 3] = (Math.random() - 0.5) * 40;
      pPos[i * 3 + 1] = Math.random() * 20 - 2;
      pPos[i * 3 + 2] = (Math.random() - 0.5) * 40;
      pVel[i] = 0.03 + Math.random() * 0.06;
    }
    pGeo.setAttribute("position", new THREE.BufferAttribute(pPos, 3));
    const pMat = new THREE.PointsMaterial({ color: 0x7c3aed, size: 0.07, transparent: true, opacity: 0.55, sizeAttenuation: true });
    scene.add(new THREE.Points(pGeo, pMat));

    /* ════════════════════════════════════════
       ORBITING NODES (small spheres)
    ════════════════════════════════════════ */
    const orbitCount = 6;
    const orbitNodes: { mesh: THREE.Mesh; angle: number; speed: number; r: number; y: number }[] = [];
    const nodeColors = [0x8b5cf6, 0x06b6d4, 0xa78bfa, 0x22d3ee];

    for (let i = 0; i < orbitCount; i++) {
      const geo = new THREE.SphereGeometry(0.18, 8, 8);
      const mat = new THREE.MeshBasicMaterial({ color: nodeColors[i % nodeColors.length] });
      const mesh = new THREE.Mesh(geo, mat);
      scene.add(mesh);

      const rGeo = new THREE.RingGeometry(0.28, 0.33, 16);
      const rMat = new THREE.MeshBasicMaterial({ color: nodeColors[i % nodeColors.length], side: THREE.DoubleSide, transparent: true, opacity: 0.5 });
      const ring = new THREE.Mesh(rGeo, rMat);
      ring.rotation.x = Math.PI / 2;
      mesh.add(ring);

      orbitNodes.push({
        mesh, angle: (i / orbitCount) * Math.PI * 2,
        speed: 0.005 + Math.random() * 0.008,
        r: 6 + Math.random() * 3,
        y: 0.5 + Math.random() * 2,
      });
    }

    /* ── Mouse parallax ── */
    let mx = 0, my = 0;
    const onMouse = (e: MouseEvent) => {
      mx = (e.clientX / window.innerWidth - 0.5) * 2;
      my = (e.clientY / window.innerHeight - 0.5) * 2;
    };
    window.addEventListener("mousemove", onMouse);

    /* ════════════════════════════════════════
       ANIMATION LOOP
    ════════════════════════════════════════ */
    let raf = 0, t = 0;
    const violetC = new THREE.Color(0x7c3aed);
    const cyanC = new THREE.Color(0x06b6d4);
    const tmp = new THREE.Color();

    function animate() {
      raf = requestAnimationFrame(animate);
      t += 0.013;

      // Car float + rotate
      carGroup.position.y = -0.2 + Math.sin(t * 0.7) * 0.25;
      carGroup.rotation.y = Math.PI * 0.15 + Math.sin(t * 0.4) * 0.18 + mx * 0.15;

      // Wheel spin
      wheelMeshes.forEach((w) => { w.rotation.x += 0.04; });

      // Wave grid
      const pa = waveGeo.attributes.position.array as Float32Array;
      const ca = waveGeo.attributes.color.array as Float32Array;
      for (let i = 0; i < pa.length / 3; i++) {
        const x = pa[i * 3], z = pa[i * 3 + 2];
        const h = Math.sin(x * 0.35 + t) * 0.55 + Math.sin(z * 0.28 + t * 0.9) * 0.45;
        pa[i * 3 + 1] = origY[i] + h;
        tmp.lerpColors(violetC, cyanC, Math.max(0, Math.min(1, (h + 1) / 2)));
        ca[i * 3] = tmp.r; ca[i * 3 + 1] = tmp.g; ca[i * 3 + 2] = tmp.b;
      }
      waveGeo.attributes.position.needsUpdate = true;
      waveGeo.attributes.color.needsUpdate = true;

      // Particles
      const pp = pGeo.attributes.position.array as Float32Array;
      for (let i = 0; i < PCOUNT; i++) {
        pp[i * 3 + 1] -= pVel[i];
        if (pp[i * 3 + 1] < -4) {
          pp[i * 3 + 1] = 16;
          pp[i * 3] = (Math.random() - 0.5) * 40;
          pp[i * 3 + 2] = (Math.random() - 0.5) * 40;
        }
      }
      pGeo.attributes.position.needsUpdate = true;

      // Orbit nodes
      orbitNodes.forEach((n) => {
        n.angle += n.speed;
        n.mesh.position.set(Math.cos(n.angle) * n.r, n.y + Math.sin(t + n.angle) * 0.4, Math.sin(n.angle) * n.r);
      });

      // Camera parallax
      camera.position.x += (mx * 2.5 - camera.position.x) * 0.04;
      camera.position.y += (6 + my * -1.5 - camera.position.y) * 0.04;
      camera.lookAt(0, 1, 0);

      renderer.render(scene, camera);
    }

    animate();

    const ro = new ResizeObserver(() => {
      if (!el) return;
      camera.aspect = el.clientWidth / el.clientHeight;
      camera.updateProjectionMatrix();
      renderer.setSize(el.clientWidth, el.clientHeight);
    });
    ro.observe(el);

    return () => {
      cancelAnimationFrame(raf);
      ro.disconnect();
      window.removeEventListener("mousemove", onMouse);
      renderer.dispose();
      if (el.contains(renderer.domElement)) el.removeChild(renderer.domElement);
    };
  }, []);

  return <div ref={mountRef} className="absolute inset-0 w-full h-full" />;
}
