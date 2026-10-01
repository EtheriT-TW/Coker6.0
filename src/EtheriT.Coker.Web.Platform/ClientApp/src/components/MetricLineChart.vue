<script setup lang="ts">
import { computed, ref, watch } from "vue";

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
const hoveredTime = ref<number | null>(null);
const samples = computed(() => {
  const byTime = new Map<number, { Name: string; Color: string; At: string; Value: number }[]>();
  for (const series of props.series) {
    for (const point of series.Points) {
      const time = new Date(point.At).getTime();
      if (!Number.isFinite(time) || point.Value === null || !Number.isFinite(point.Value)) continue;
      const rows = byTime.get(time) ?? [];
      rows.push({ Name: series.Name, Color: series.Color, At: point.At, Value: point.Value });
      byTime.set(time, rows);
    }
  }
  return [...byTime].sort((a, b) => a[0] - b[0]);
});
const hoveredSample = computed(() => samples.value.find(([time]) => time === hoveredTime.value));
const hoverX = computed(() => hoveredSample.value ? xPosition(hoveredSample.value[1][0].At) : left);

function updateHover(event: PointerEvent): void {
  const svg = event.currentTarget as SVGSVGElement;
  const matrix = svg.getScreenCTM();
  if (!matrix || !samples.value.length) { hoveredTime.value = null; return; }
  const cursor = svg.createSVGPoint();
  cursor.x = event.clientX;
  cursor.y = event.clientY;
  const position = cursor.matrixTransform(matrix.inverse());
  if (position.x < left || position.x > width - right || position.y < top || position.y > height - bottom) {
    hoveredTime.value = null;
    return;
  }
  const time = minTime.value + (position.x - left) / (width - left - right) * (maxTime.value - minTime.value);
  hoveredTime.value = samples.value.reduce((nearest, sample) =>
    Math.abs(sample[0] - time) < Math.abs(nearest[0] - time) ? sample : nearest)[0];
}

watch(samples, () => { hoveredTime.value = null; });

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
    <div v-if="hasData" class="chart-plot">
    <svg :viewBox="`0 0 ${width} ${height}`" role="img" aria-label="使用率趨勢圖" @pointermove="updateHover" @pointerleave="hoveredTime = null" @pointercancel="hoveredTime = null">
      <g class="grid">
        <template v-for="value in [0, 25, 50, 75, 100]" :key="value">
          <line :x1="left" :x2="width - right" :y1="yPosition(value)" :y2="yPosition(value)" />
          <text :x="left - 8" :y="yPosition(value) + 4" text-anchor="end">{{ value }}%</text>
        </template>
      </g>
      <polyline v-for="item in series" :key="item.Name" :points="points(item)" :stroke="item.Color" />
      <g v-if="hoveredSample" class="hover-markers">
        <line :x1="hoverX" :x2="hoverX" :y1="top" :y2="height - bottom" />
        <circle v-for="item in hoveredSample[1]" :key="item.Name" :cx="hoverX" :cy="yPosition(item.Value)" r="5" :fill="item.Color" />
      </g>
      <text class="edge-time" :x="left" :y="height - 8">{{ formatEdgeTime(minTime) }}</text>
      <text class="edge-time" :x="width - right" :y="height - 8" text-anchor="end">{{ formatEdgeTime(maxTime) }}</text>
    </svg>
    <div v-if="hoveredSample" class="chart-tooltip" :style="{ left: `${hoverX / width * 100}%`, transform: hoverX > width / 2 ? 'translateX(-100%)' : 'none' }" role="tooltip">
      <strong>{{ new Date(hoveredSample[0]).toLocaleString("zh-TW", { year: "numeric", month: "2-digit", day: "2-digit", hour: "2-digit", minute: "2-digit" }) }}</strong>
      <span v-for="item in hoveredSample[1]" :key="item.Name"><i :style="{ backgroundColor: item.Color }"></i>{{ item.Name }}：{{ item.Value.toFixed(1) }}%</span>
    </div>
    </div>
    <p v-else class="empty-chart">{{ emptyText ?? "目前還沒有歷史資料" }}</p>
  </div>
</template>

<style scoped>
.chart-plot { position: relative; }.chart-tooltip { position: absolute; top: 0; z-index: 1; display: grid; gap: .35rem; padding: .65rem .8rem; border: 1px solid #cfd6e2; border-radius: 6px; background: #fff; color: #234; box-shadow: 0 3px 12px #0002; font-size: .82rem; white-space: nowrap; pointer-events: none; }.chart-tooltip span { display: flex; align-items: center; gap: .35rem; }.chart-tooltip i { width: .65rem; height: .65rem; border-radius: 50%; }.hover-markers { pointer-events: none; }.hover-markers line { stroke: #94a3b8; stroke-dasharray: 4 4; vector-effect: non-scaling-stroke; }.hover-markers circle { stroke: #fff; stroke-width: 2; vector-effect: non-scaling-stroke; }
.metric-chart { min-height: 270px; }.chart-legend { display: flex; flex-wrap: wrap; gap: .6rem 1.1rem; margin-bottom: .4rem; color: #596579; font-size: .82rem; }.chart-legend span { display: inline-flex; align-items: center; gap: .35rem; }.chart-legend i { width: .65rem; height: .65rem; border-radius: 50%; }svg { width: 100%; height: auto; overflow: visible; }.grid line { stroke: #e6eaf0; stroke-width: 1; }.grid text, .edge-time { fill: #7a8496; font-size: 12px; }polyline { fill: none; stroke-width: 2.5; stroke-linejoin: round; stroke-linecap: round; vector-effect: non-scaling-stroke; }.empty-chart { display: grid; place-items: center; min-height: 220px; color: #7a8496; }
</style>
