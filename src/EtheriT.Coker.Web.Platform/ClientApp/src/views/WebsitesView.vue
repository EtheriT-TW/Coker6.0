<script setup lang="ts">
import { ref } from "vue";
import { DxColumn, DxDataGrid } from "devextreme-vue/data-grid";

interface WebsiteRow {
  Id: number;
  CompanyName: string;
  WebsiteName: string;
  Environment: string;
  Status: string;
}

const websites = ref<WebsiteRow[]>([]);
</script>

<template>
  <section class="page-heading">
    <div><h1>網站與站台</h1><p>查看各客戶擁有的網站、環境與目前運作狀態。</p></div>
  </section>
  <section class="data-card">
    <DxDataGrid
      :data-source="websites"
      key-expr="Id"
      :show-borders="true"
      :column-auto-width="true"
      no-data-text="尚未接入站台資料"
    >
      <DxColumn data-field="CompanyName" caption="客戶" />
      <DxColumn data-field="WebsiteName" caption="站台名稱" />
      <DxColumn data-field="Environment" caption="環境" />
      <DxColumn data-field="Status" caption="狀態" />
    </DxDataGrid>
  </section>
</template>
