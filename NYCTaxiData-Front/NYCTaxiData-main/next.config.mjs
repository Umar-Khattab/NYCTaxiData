import './monkeypatch.js';

/** @type {import('next').NextConfig} */
const nextConfig = {
  typescript: {
    ignoreBuildErrors: true,
  },
  images: {
    unoptimized: true,
  },
  async redirects() {
    return [
      {
        source: '/heatmap',
        destination: '/dispatch',
        permanent: true,
      },
    ]
  },
}

export default nextConfig
