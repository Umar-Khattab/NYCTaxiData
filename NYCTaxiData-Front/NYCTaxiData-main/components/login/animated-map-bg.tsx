"use client";

import { useEffect, useRef } from "react";

interface Taxi {
  x: number;
  y: number;
  vx: number;
  vy: number;
  trail: { x: number; y: number }[];
  color: string;
  speed: number;
}

interface Pulse {
  x: number;
  y: number;
  radius: number;
  maxRadius: number;
  alpha: number;
  color: string;
}

interface DataStream {
  points: { x: number; y: number }[];
  progress: number;
  speed: number;
  color: string;
  width: number;
}

export function AnimatedMapBg() {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const animRef = useRef<number>(0);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    let width = canvas.offsetWidth;
    let height = canvas.offsetHeight;
    canvas.width = width;
    canvas.height = height;

    const AMBER = "#f59e0b";
    const AMBER_DIM = "#d97706";
    const CYAN = "#06b6d4";
    const PURPLE = "#818cf8";
    const GREEN = "#10b981";

    // ── Grid (street-grid effect) ──
    function drawGrid() {
      ctx!.strokeStyle = "rgba(99, 102, 241, 0.06)";
      ctx!.lineWidth = 1;
      const step = 36;
      for (let x = 0; x < width; x += step) {
        ctx!.beginPath();
        ctx!.moveTo(x, 0);
        ctx!.lineTo(x, height);
        ctx!.stroke();
      }
      for (let y = 0; y < height; y += step) {
        ctx!.beginPath();
        ctx!.moveTo(0, y);
        ctx!.lineTo(width, y);
        ctx!.stroke();
      }
    }

    // ── Heatmap blobs ──
    const heatBlobs = Array.from({ length: 6 }, () => ({
      x: Math.random() * width,
      y: Math.random() * height,
      r: 80 + Math.random() * 120,
      color: Math.random() > 0.5 ? AMBER : CYAN,
    }));

    function drawHeatmap() {
      heatBlobs.forEach((b) => {
        const grad = ctx!.createRadialGradient(b.x, b.y, 0, b.x, b.y, b.r);
        grad.addColorStop(0, b.color.replace(")", ", 0.18)").replace("rgb", "rgba").replace("#f59e0b", "rgba(245,158,11,0.18)").replace("#06b6d4", "rgba(6,182,212,0.10)"));
        const c0 = b.color === AMBER ? "rgba(245,158,11,0.18)" : "rgba(6,182,212,0.10)";
        const c1 = "rgba(0,0,0,0)";
        const g2 = ctx!.createRadialGradient(b.x, b.y, 0, b.x, b.y, b.r);
        g2.addColorStop(0, c0);
        g2.addColorStop(1, c1);
        ctx!.fillStyle = g2;
        ctx!.beginPath();
        ctx!.arc(b.x, b.y, b.r, 0, Math.PI * 2);
        ctx!.fill();
      });
    }

    // ── Taxis ──
    const taxiColors = [AMBER, AMBER_DIM, CYAN, AMBER];
    const taxis: Taxi[] = Array.from({ length: 22 }, () => ({
      x: Math.random() * width,
      y: Math.random() * height,
      vx: (Math.random() - 0.5) * 1.4,
      vy: (Math.random() - 0.5) * 1.4,
      trail: [],
      color: taxiColors[Math.floor(Math.random() * taxiColors.length)],
      speed: 0.6 + Math.random() * 0.8,
    }));

    function updateTaxis() {
      taxis.forEach((t) => {
        t.trail.push({ x: t.x, y: t.y });
        if (t.trail.length > 28) t.trail.shift();
        t.x += t.vx * t.speed;
        t.y += t.vy * t.speed;
        if (t.x < 0 || t.x > width) t.vx *= -1;
        if (t.y < 0 || t.y > height) t.vy *= -1;
      });
    }

    function drawTaxis() {
      taxis.forEach((t) => {
        // trail
        for (let i = 1; i < t.trail.length; i++) {
          const alpha = i / t.trail.length;
          ctx!.strokeStyle = t.color === AMBER
            ? `rgba(245,158,11,${alpha * 0.6})`
            : `rgba(6,182,212,${alpha * 0.5})`;
          ctx!.lineWidth = alpha * 2.2;
          ctx!.beginPath();
          ctx!.moveTo(t.trail[i - 1].x, t.trail[i - 1].y);
          ctx!.lineTo(t.trail[i].x, t.trail[i].y);
          ctx!.stroke();
        }
        // dot
        const glow = ctx!.createRadialGradient(t.x, t.y, 0, t.x, t.y, 7);
        const isAmber = t.color === AMBER || t.color === AMBER_DIM;
        glow.addColorStop(0, isAmber ? "rgba(245,158,11,0.95)" : "rgba(6,182,212,0.95)");
        glow.addColorStop(1, "rgba(0,0,0,0)");
        ctx!.fillStyle = glow;
        ctx!.beginPath();
        ctx!.arc(t.x, t.y, 7, 0, Math.PI * 2);
        ctx!.fill();
        // core
        ctx!.fillStyle = t.color;
        ctx!.beginPath();
        ctx!.arc(t.x, t.y, 2.5, 0, Math.PI * 2);
        ctx!.fill();
      });
    }

    // ── Pulse rings (hotspot zones) ──
    const pulses: Pulse[] = [];
    const pulseColors = [AMBER, CYAN, PURPLE, GREEN];
    function spawnPulse() {
      pulses.push({
        x: Math.random() * width,
        y: Math.random() * height,
        radius: 0,
        maxRadius: 60 + Math.random() * 70,
        alpha: 0.9,
        color: pulseColors[Math.floor(Math.random() * pulseColors.length)],
      });
    }
    setInterval(spawnPulse, 1200);

    function drawPulses() {
      for (let i = pulses.length - 1; i >= 0; i--) {
        const p = pulses[i];
        p.radius += 0.9;
        p.alpha = Math.max(0, 1 - p.radius / p.maxRadius);
        if (p.alpha <= 0) { pulses.splice(i, 1); continue; }
        const rgb = p.color === AMBER ? "245,158,11"
          : p.color === CYAN ? "6,182,212"
          : p.color === PURPLE ? "129,140,248"
          : "16,185,129";
        ctx!.strokeStyle = `rgba(${rgb},${p.alpha * 0.8})`;
        ctx!.lineWidth = 1.5;
        ctx!.beginPath();
        ctx!.arc(p.x, p.y, p.radius, 0, Math.PI * 2);
        ctx!.stroke();
      }
    }

    // ── Data streams (diagonal bright lines) ──
    function makeStream(): DataStream {
      const isH = Math.random() > 0.5;
      const pts = isH
        ? [{ x: 0, y: Math.random() * height }, { x: width, y: Math.random() * height }]
        : [{ x: Math.random() * width, y: 0 }, { x: Math.random() * width, y: height }];
      return {
        points: pts,
        progress: 0,
        speed: 0.003 + Math.random() * 0.005,
        color: Math.random() > 0.6 ? AMBER : Math.random() > 0.5 ? CYAN : PURPLE,
        width: 0.5 + Math.random() * 1.2,
      };
    }
    const streams: DataStream[] = Array.from({ length: 12 }, makeStream);

    function drawStreams() {
      streams.forEach((s) => {
        s.progress = Math.min(1, s.progress + s.speed);
        const [a, b] = s.points;
        const ex = a.x + (b.x - a.x) * s.progress;
        const ey = a.y + (b.y - a.y) * s.progress;
        const grad = ctx!.createLinearGradient(a.x, a.y, ex, ey);
        const rgb = s.color === AMBER ? "245,158,11"
          : s.color === CYAN ? "6,182,212"
          : "129,140,248";
        grad.addColorStop(0, `rgba(${rgb},0)`);
        grad.addColorStop(0.6, `rgba(${rgb},0.35)`);
        grad.addColorStop(1, `rgba(${rgb},0.8)`);
        ctx!.strokeStyle = grad;
        ctx!.lineWidth = s.width;
        ctx!.beginPath();
        ctx!.moveTo(a.x, a.y);
        ctx!.lineTo(ex, ey);
        ctx!.stroke();
        if (s.progress >= 1) Object.assign(s, { ...makeStream(), progress: 0 });
      });
    }

    // ── NYC-style borough labels ──
    const labels = [
      { text: "MANHATTAN", x: 0.45, y: 0.18 },
      { text: "BROOKLYN", x: 0.55, y: 0.72 },
      { text: "QUEENS", x: 0.78, y: 0.38 },
      { text: "BRONX", x: 0.3, y: 0.08 },
    ];

    function drawLabels() {
      labels.forEach((l) => {
        ctx!.font = "bold 11px 'Geist Mono', monospace";
        ctx!.letterSpacing = "3px";
        ctx!.fillStyle = "rgba(245,158,11,0.22)";
        ctx!.fillText(l.text, l.x * width, l.y * height);
      });
    }

    // ── Corner overlay (vignette) ──
    function drawVignette() {
      const vg = ctx!.createRadialGradient(
        width / 2, height / 2, height * 0.2,
        width / 2, height / 2, height * 0.9
      );
      vg.addColorStop(0, "rgba(7,11,21,0)");
      vg.addColorStop(1, "rgba(7,11,21,0.75)");
      ctx!.fillStyle = vg;
      ctx!.fillRect(0, 0, width, height);
    }

    // ── Main loop ──
    function render() {
      ctx!.clearRect(0, 0, width, height);

      // base dark background
      ctx!.fillStyle = "#070b15";
      ctx!.fillRect(0, 0, width, height);

      drawGrid();
      drawHeatmap();
      drawStreams();
      updateTaxis();
      drawTaxis();
      drawPulses();
      drawLabels();
      drawVignette();

      animRef.current = requestAnimationFrame(render);
    }

    render();

    // ── Resize ──
    const ro = new ResizeObserver(() => {
      width = canvas.offsetWidth;
      height = canvas.offsetHeight;
      canvas.width = width;
      canvas.height = height;
    });
    ro.observe(canvas);

    return () => {
      cancelAnimationFrame(animRef.current);
      ro.disconnect();
      clearInterval(0);
    };
  }, []);

  return (
    <canvas
      ref={canvasRef}
      className="absolute inset-0 w-full h-full"
      style={{ display: "block" }}
    />
  );
}
