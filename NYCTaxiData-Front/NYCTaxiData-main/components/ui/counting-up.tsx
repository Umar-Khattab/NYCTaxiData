'use client'

import { useEffect, useState } from 'react'

interface CountingUpProps {
  value: number
  duration?: number // in ms, defaults to 1500
  prefix?: string
  suffix?: string
  decimals?: number
  className?: string
}

export function CountingUp({
  value,
  duration = 1500,
  prefix = '',
  suffix = '',
  decimals = 0,
  className = '',
}: CountingUpProps) {
  const [displayValue, setDisplayValue] = useState(0)

  useEffect(() => {
    let startTimestamp: number | null = null
    const startValue = 0

    const step = (timestamp: number) => {
      if (!startTimestamp) startTimestamp = timestamp
      const progress = Math.min((timestamp - startTimestamp) / duration, 1)
      
      // Easing: easeOutQuad
      const easeProgress = progress * (2 - progress)
      
      const currentValue = startValue + easeProgress * (value - startValue)
      setDisplayValue(currentValue)

      if (progress < 1) {
        window.requestAnimationFrame(step)
      } else {
        setDisplayValue(value)
      }
    }

    window.requestAnimationFrame(step)
  }, [value, duration])

  const formattedValue = displayValue.toLocaleString(undefined, {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  })

  return (
    <span className={className}>
      {prefix}
      {formattedValue}
      {suffix}
    </span>
  )
}
