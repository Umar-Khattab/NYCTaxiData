"use client";

import { useEffect, useState, useRef } from "react";

const BOOT_LINES = [
  "INITIALIZING FLEET COMMAND SYSTEMS...",
  "CONNECTING TO NYC METRO GRID...",
  "LOADING TELEMETRY ENGINE v4.2.1...",
  "AUTHENTICATING SECURE CHANNEL...",
  "SYNCING 2,847 ACTIVE VEHICLES...",
  "COMMAND CENTER READY.",
];

interface FleetLoaderProps {
  onComplete: () => void;
}

export function FleetLoader({ onComplete }: FleetLoaderProps) {
  const [progress, setProgress] = useState(0);
  const [lineIdx, setLineIdx] = useState(0);
  const [displayedText, setDisplayedText] = useState("");
  const [charIdx, setCharIdx] = useState(0);
  const [phase, setPhase] = useState<"boot" | "fadeout">("boot");
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const rafRef = useRef<number>(0);

  /* ── Hex grid background ── */
  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;

    const HEX_SIZE = 28;
    const cols = Math.ceil(canvas.width / (HEX_SIZE * 1.73)) + 2;
    const rows = Math.ceil(canvas.height / (HEX_SIZE * 1.5)) + 2;

    let t = 0;
    function drawHex(cx: number, cy: number, alpha: number) {
      ctx!.beginPath();
      for (let i = 0; i < 6; i++) {
        const angle = (Math.PI / 3) * i - Math.PI / 6;
        const x = cx + HEX_SIZE * Math.cos(angle);
        const y = cy + HEX_SIZE * Math.sin(angle);
        i === 0 ? ctx!.moveTo(x, y) : ctx!.lineTo(x, y);
      }
      ctx!.closePath();
      ctx!.strokeStyle = `rgba(124,58,237,${alpha})`;
      ctx!.lineWidth = 0.6;
      ctx!.stroke();
    }

    function render() {
      ctx!.clearRect(0, 0, canvas!.width, canvas!.height);
      ctx!.fillStyle = "#04020a";
      ctx!.fillRect(0, 0, canvas!.width, canvas!.height);

      t += 0.012;
      for (let row = 0; row < rows; row++) {
        for (let col = 0; col < cols; col++) {
          const cx = col * HEX_SIZE * 1.73 + (row % 2) * HEX_SIZE * 0.866;
          const cy = row * HEX_SIZE * 1.5;
          const wave = Math.sin(t + col * 0.3 + row * 0.2) * 0.5 + 0.5;
          drawHex(cx, cy, wave * 0.12 + 0.03);
        }
      }

      // Scanning beam
      const beamY = ((t * 80) % (canvas!.height + 100)) - 50;
      const beamGrad = ctx!.createLinearGradient(0, beamY - 60, 0, beamY + 60);
      beamGrad.addColorStop(0, "rgba(124,58,237,0)");
      beamGrad.addColorStop(0.5, "rgba(124,58,237,0.08)");
      beamGrad.addColorStop(1, "rgba(124,58,237,0)");
      ctx!.fillStyle = beamGrad;
      ctx!.fillRect(0, beamY - 60, canvas!.width, 120);

      // Cyan scan line
      ctx!.strokeStyle = `rgba(6,182,212,0.4)`;
      ctx!.lineWidth = 1;
      ctx!.beginPath();
      ctx!.moveTo(0, beamY);
      ctx!.lineTo(canvas!.width, beamY);
      ctx!.stroke();

      rafRef.current = requestAnimationFrame(render);
    }
    render();

    const onResize = () => {
      canvas.width = window.innerWidth;
      canvas.height = window.innerHeight;
    };
    window.addEventListener("resize", onResize);
    return () => {
      cancelAnimationFrame(rafRef.current);
      window.removeEventListener("resize", onResize);
    };
  }, []);

  /* ── Typing effect ── */
  useEffect(() => {
    if (lineIdx >= BOOT_LINES.length) return;
    const line = BOOT_LINES[lineIdx];
    if (charIdx < line.length) {
      const t = setTimeout(() => {
        setDisplayedText(line.slice(0, charIdx + 1));
        setCharIdx((c) => c + 1);
      }, 28);
      return () => clearTimeout(t);
    } else {
      const t = setTimeout(() => {
        setLineIdx((i) => i + 1);
        setCharIdx(0);
        setDisplayedText("");
      }, 280);
      return () => clearTimeout(t);
    }
  }, [charIdx, lineIdx]);

  /* ── Progress bar ── */
  useEffect(() => {
    const target = Math.min(((lineIdx + 1) / BOOT_LINES.length) * 100, 100);
    const interval = setInterval(() => {
      setProgress((p) => {
        if (p >= target) { clearInterval(interval); return p; }
        return Math.min(p + 1.2, target);
      });
    }, 16);
    return () => clearInterval(interval);
  }, [lineIdx]);

  /* ── Trigger fade-out when done ── */
  useEffect(() => {
    if (lineIdx >= BOOT_LINES.length && progress >= 99) {
      const t = setTimeout(() => setPhase("fadeout"), 400);
      return () => clearTimeout(t);
    }
  }, [lineIdx, progress]);

  useEffect(() => {
    if (phase === "fadeout") {
      const t = setTimeout(onComplete, 700);
      return () => clearTimeout(t);
    }
  }, [phase, onComplete]);

  return (
    <div
      className="fixed inset-0 z-50 flex flex-col items-center justify-center overflow-hidden"
      style={{
        transition: "opacity 0.7s ease",
        opacity: phase === "fadeout" ? 0 : 1,
        pointerEvents: phase === "fadeout" ? "none" : "all",
      }}
    >
      {/* Hex canvas bg */}
      <canvas ref={canvasRef} className="absolute inset-0 w-full h-full" />

      {/* Radial vignette */}
      <div
        className="absolute inset-0"
        style={{
          background:
            "radial-gradient(ellipse at center, rgba(124,58,237,0.08) 0%, rgba(4,2,10,0.7) 70%)",
        }}
      />

      {/* Content */}
      <div className="relative z-10 flex flex-col items-center w-full max-w-lg px-8">
        {/* Logo */}
        <div className="flex flex-col items-center mb-10">
          <div
            className="w-20 h-20 rounded-2xl flex items-center justify-center mb-4"
            style={{
              background: "linear-gradient(135deg, rgba(124,58,237,0.2), rgba(79,70,229,0.1))",
              border: "1px solid rgba(124,58,237,0.4)",
              boxShadow: "0 0 60px rgba(124,58,237,0.3), inset 0 0 30px rgba(124,58,237,0.05)",
              animation: "logoPulse 2s ease-in-out infinite",
            }}
          >
            <span
              className="text-4xl"
              style={{ filter: "drop-shadow(0 0 12px rgba(124,58,237,0.8))" }}
            >
              <svg viewBox="0 0 24 24" fill="none" className="w-9 h-9" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" style={{ filter: "drop-shadow(0 0 10px rgba(124,58,237,0.9))" }}>
              <rect x="2" y="11" width="20" height="8" rx="2"/>
              <path d="M5 11V9a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v2"/>
              <path d="M7 7l1.5-3h7L17 7"/>
              <circle cx="7" cy="19" r="1.5" fill="currentColor"/>
              <circle cx="17" cy="19" r="1.5" fill="currentColor"/>
              <path d="M10 8h4"/>
            </svg>
            </span>
          </div>
          <div
            className="text-2xl font-bold tracking-[0.3em] uppercase"
            style={{
              background: "linear-gradient(90deg, #8b5cf6, #a78bfa, #06b6d4)",
              WebkitBackgroundClip: "text",
              WebkitTextFillColor: "transparent",
              filter: "drop-shadow(0 0 8px rgba(124,58,237,0.4))",
            }}
          >
            FleetCommand
          </div>
          <div className="text-[10px] tracking-[0.5em] text-violet-500 uppercase mt-1">
            NYC Operations Center
          </div>
        </div>

        {/* Terminal boot log */}
        <div
          className="w-full rounded-xl mb-6 p-5 font-mono text-xs overflow-hidden"
          style={{
            background: "rgba(6,3,13,0.85)",
            border: "1px solid rgba(124,58,237,0.2)",
            minHeight: 160,
          }}
        >
          {BOOT_LINES.slice(0, lineIdx).map((line, i) => (
            <div key={i} className="flex items-center gap-2 mb-1.5">
              <span className="text-emerald-500">✓</span>
              <span className="text-violet-400/60">{line}</span>
            </div>
          ))}
          {lineIdx < BOOT_LINES.length && (
            <div className="flex items-center gap-2">
              <span
                className="text-cyan-400"
                style={{ animation: "blink 0.7s step-end infinite" }}
              >
                ▶
              </span>
              <span className="text-cyan-300">
                {displayedText}
                <span
                  className="inline-block w-2 h-3 bg-cyan-400 ml-0.5 align-middle"
                  style={{ animation: "blink 0.6s step-end infinite" }}
                />
              </span>
            </div>
          )}
        </div>

        {/* Progress bar */}
        <div className="w-full">
          <div className="flex justify-between items-center mb-2">
            <span className="text-[10px] text-violet-500 font-mono tracking-widest uppercase">
              System Boot
            </span>
            <span className="text-[10px] text-cyan-400 font-mono font-bold">
              {Math.round(progress)}%
            </span>
          </div>
          <div
            className="w-full h-1.5 rounded-full overflow-hidden"
            style={{ background: "rgba(124,58,237,0.1)", border: "1px solid rgba(124,58,237,0.15)" }}
          >
            <div
              className="h-full rounded-full transition-all duration-150"
              style={{
                width: `${progress}%`,
                background: "linear-gradient(90deg, #7c3aed, #4f46e5, #06b6d4)",
                boxShadow: "0 0 12px rgba(124,58,237,0.6)",
              }}
            />
          </div>
          {/* Segment ticks */}
          <div className="flex justify-between mt-1.5">
            {[0, 25, 50, 75, 100].map((v) => (
              <div
                key={v}
                className="flex flex-col items-center"
              >
                <div
                  className="w-px h-1.5"
                  style={{
                    background:
                      progress >= v
                        ? "rgba(124,58,237,0.8)"
                        : "rgba(124,58,237,0.2)",
                  }}
                />
                <span
                  className="text-[8px] font-mono mt-0.5"
                  style={{
                    color:
                      progress >= v
                        ? "rgba(167,139,250,0.7)"
                        : "rgba(124,58,237,0.3)",
                  }}
                >
                  {v}%
                </span>
              </div>
            ))}
          </div>
        </div>

        {/* Bottom status */}
        <div className="flex items-center gap-3 mt-6 text-[10px] font-mono text-violet-700">
          <div
            className="w-1.5 h-1.5 rounded-full bg-emerald-400"
            style={{ boxShadow: "0 0 6px #10b981", animation: "blink 1.2s step-end infinite" }}
          />
          <span>SECURE TLS 1.3</span>
          <span className="text-violet-800">//</span>
          <span>SOC 2 TYPE II</span>
          <span className="text-violet-800">//</span>
          <span>v4.2.1-stable</span>
        </div>
      </div>

      <style>{`
        @keyframes logoPulse {
          0%, 100% { box-shadow: 0 0 60px rgba(124,58,237,0.3), inset 0 0 30px rgba(124,58,237,0.05); }
          50%       { box-shadow: 0 0 90px rgba(124,58,237,0.55), inset 0 0 40px rgba(124,58,237,0.1); }
        }
        @keyframes blink {
          0%, 100% { opacity: 1; }
          50%       { opacity: 0; }
        }
      `}</style>
    </div>
  );
}
