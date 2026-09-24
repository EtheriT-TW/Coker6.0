<script setup lang="ts">
import { computed } from "vue";

interface MetricChartPoint { At: string; Value: number | null }
interface MetricChartSeries { Name: string; Color: string; Points: MetricChartPoint[] }

const props = defineProps<{ series: MetricChartSeries[]; emptyText?: string }>();
const width = 1000;
const height = 250;
const left = 46;
const right = 18;
const top = 16;
const bottom = 34;
const allTimes = computed(() => props.series.flatMap(x => x.Points).map(x => new Date(x.At).getTime()).filter(Number.isFinite));
const minTime = computed(() => allTimes.value.length ? Math.min(...allTimes.value) : 0);
const maxTime = computed(() => allTimes.value.length ? Math.max(...allTimes.value) : 0);
const hasData = computed(() => props.series.some(x => x.Points.some(y => y.Value !== null)));

function xPosition(at: string): number {
  const range = maxTime.value - minTime.value;
  return range <= 0 ? left : left + (new Date(at).getTime() - minTime.value) / range * (width - left - right);
}

function yPosition(value: number): number {
  return top + (100 - Math.max(0, Math.min(100, value))) / 100 * (height - top - bottom);
}

function points(series: MetricChartSeries): string {
  return series.Points.filter(x => x.Value !== null)
    .map(x => `${xPosition(x.At).toFixed(1)},${yPosition(x.Value!).toFixed(1)}`)
    .join(" ");
}

function formatEdgeTime(value: number): string {
  return value ? new Date(value).toLocaleString("zh-TW", { month: "2-digit", day: "2-digit", hour: "2-digit", minute: "2-digit" }) : "";
}
</script>

<template>
  <div class="metric-chart">
    <div class="chart-legend"><span v-for="item in series" :key="item.Name"><i :style="{ backgroundColor: item.Color }"></i>{{ item.Name }}</span></div>
    <svg v-if="hasData" :viewBox="`0 0 ${width} ${height}`" role="img" aria-label="使用率趨勢圖">
      <g class="grid">
        <template v-for="value in [0, 25, 50, 75, 100]" :key="value">
          <line :x1="left" :x2="width - right" :y1="yPosition(value)" :y2="yPosition(value)" />
          <text :x="left - 8" :y="yPosition(value) + 4" text-anchor="end">{{ value }}%</text>
        </template>
      </g>
      <polyline v-for="item in series" :key="item.Name" :points="points(item)" :stroke="item.Color" />
      <text class="edge-time" :x="left" :y="height - 8">{{ formatEdgeTime(minTime) }}</text>
      <text class="edge-time" :x="width - right" :y="height - 8" text-anchor="end">{{ formatEdgeTime(maxTime) }}</text>
    </svg>
    <p v-else class="empty-chart">{{ emptyText ?? "目前還沒有歷史資料" }}</p>
  </div>
</template>

<style scoped>
.metric-chart { min-height: 270px; }.chart-legend { display: flex; flex-wrap: wrap; gap: .6rem 1.1rem; margin-bottom: .4rem; color: #596579; font-size: .82rem; }.chart-legend span { display: inline-flex; align-items: center; gap: .35rem; }.chart-legend i { width: .65rem; height: .65rem; border-radius: 50%; }svg { width: 100%; height: auto; overflow: visible; }.grid line { stroke: #e6eaf0; stroke-width: 1; }.grid text, .edge-time { fill: #7a8496; font-size: 12px; }polyline { fill: none; stroke-width: 2.5; stroke-linejoin: round; stroke-linecap: round; vector-effect: non-scaling-stroke; }.empty-chart { display: grid; place-items: center; min-height: 220px; color: #7a8496; }
</style>
