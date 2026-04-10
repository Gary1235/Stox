import { Component, signal, computed, input, effect } from '@angular/core';

// 定義資料介面
export interface ChartData {
  label: string | null;
  value: number;
  color?: string;
}

// 用於渲染 SVG 路徑的介面
interface SliceData extends ChartData {
  path: string;
  percent: number;
  textX: number; // 文字座標 X
  textY: number; // 文字座標 Y
}

@Component({
  selector: 'comp-pie-chart',
  imports: [],
  templateUrl: './pie-chart.component.html',
  styleUrl: './pie-chart.component.scss',
})
export class PieChartComponent {
  // 圓餅圖調色盤
  stockPalette = [
    '#3366CC', // 01. 經典藍 (科技/大型股)
    '#DC3912', // 02. 警示紅 (下跌/風險)
    '#FF9900', // 03. 活力橙 (消費/新興)
    '#109618', // 04. 獲利綠 (能源/傳產)
    '#990099', // 05. 紫羅蘭
    '#0099C6', // 06. 青藍色
    '#DD4477', // 07. 玫瑰粉
    '#66AA00', // 08. 萊姆綠
    '#B82E2E', // 09. 深磚紅
    '#316395', // 10. 鋼鐵藍
    '#994499', // 11. 薰衣草紫
    '#22AA99', // 12. 海藻綠
    '#AAAA11', // 13. 橄欖黃
    '#6633CC', // 14. 深寶藍
    '#E67300', // 15. 焦糖橙
    '#8B0707', // 16. 酒紅色
    '#329262', // 17. 森林綠
    '#5574A6', // 18. 岩石灰藍
    '#3B3EAC', // 19. 靛青色
    '#FFD700', // 20. 黃金 (特別保留給現金或貴金屬)
  ];

  // 原始資料
  rawData = input<ChartData[]>([
    { label: '前端開發', value: 40, color: '#6366f1' },
    { label: '後端架構', value: 30, color: '#ec4899' },
    { label: 'UI/UX 設計', value: 25, color: '#8b5cf6' },
    { label: '專案管理', value: 15, color: '#10b981' },
    { label: '測試維運', value: 10, color: '#f59e0b' },
  ]);

  constructor() {
    this.assignColor();
  }

  // 圓餅圖資料
  chartData = computed<ChartData[]>(() => {
    return this.rawData().map((item, idx) => {
      item.color = this.stockPalette[idx + (1 % this.stockPalette.length)];

      return {
        ...item
      }
    });
  });
  assignColor() {
    effect(() => {});
  }

  // 互動狀態
  hoveredIndex = signal<number | null>(null);

  // 計算總值
  totalValue = computed(() => {
    return this.chartData().reduce((acc, curr) => acc + curr.value, 0);
  });

  // 計算被 Hover 的資料物件
  hoveredData = computed(() => {
    const index = this.hoveredIndex();
    return index !== null ? this.chartData()[index] : null;
  });

  // 核心邏輯：將數據轉換為 SVG 路徑與文字座標
  slices = computed<SliceData[]>(() => {
    const total = this.totalValue();
    let cumulativePercent = 0;

    return this.chartData().map((item) => {
      const percent = item.value / total;

      // 計算起始角度和結束角度 (單位：弧度)
      const startAngle = 2 * Math.PI * cumulativePercent - Math.PI / 2;
      const endAngle = 2 * Math.PI * (cumulativePercent + percent) - Math.PI / 2;

      // 計算路徑
      const path = this.getPieSlicePath(startAngle, endAngle);

      // 計算文字位置 (取扇形中間角度)
      const midAngle = startAngle + (endAngle - startAngle) / 2;
      const textCoords = this.getCoordinatesForAngle(midAngle, 0.7); // 0.7 是文字距離圓心的距離(半徑比例)

      cumulativePercent += percent;

      return {
        ...item,
        percent,
        textX: textCoords.x,
        textY: textCoords.y,
        path,
      };
    });
  });

  // 互動事件處理
  setHovered(index: number | null) {
    this.hoveredIndex.set(index);
  }

  onMouseLeave() {
    this.hoveredIndex.set(null);
  }

  // --- 數學幾何計算 ---

  // 取得圓周上的點座標
  // radius 預設為 1 (圓餅圖邊緣)，傳入較小的值可以用來定位文字
  private getCoordinatesForAngle(angle: number, radius: number = 1) {
    return {
      x: Math.cos(angle) * radius,
      y: Math.sin(angle) * radius,
    };
  }

  // 產生 SVG Path 字串
  private getPieSlicePath(startAngle: number, endAngle: number): string {
    // 解決無法畫出一整個圓的問題
    // 1. 算出角度差
    const angleDiff = endAngle - startAngle;

    // 2. 修正：如果角度差大於等於 360度 (2 * PI)，SVG 會畫不出來
    // 我們故意減去一個極小值 (0.0001)，讓它變成 359.99度
    // 這樣 SVG 就能畫出一個視覺上封閉的圓
    if (angleDiff >= 2 * Math.PI - 0.0001) {
      endAngle = startAngle + 2 * Math.PI - 0.0001;
    }

    const start = this.getCoordinatesForAngle(startAngle);
    const end = this.getCoordinatesForAngle(endAngle);

    const largeArcFlag = endAngle - startAngle > Math.PI ? 1 : 0;

    return `M 0 0 L ${start.x} ${start.y} A 1 1 0 ${largeArcFlag} 1 ${end.x} ${end.y} Z`;
  }
}
