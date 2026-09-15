<script setup lang="ts">
import { ref } from "vue";
import { DxColumn, DxDataGrid } from "devextreme-vue/data-grid";

interface CompanyRow {
  Id: number;
  Name: string;
  ContactName: string;
  Status: string;
}

const companies = ref<CompanyRow[]>([]);
</script>

<template>
  <section class="page-heading">
    <div><h1>客戶資料</h1><p>管理公司基本資料，以及公司與站台之間的關係。</p></div>
  </section>
  <section class="data-card">
    <DxDataGrid
      :data-source="companies"
      key-expr="Id"
      :show-borders="true"
      :column-auto-width="true"
      no-data-text="尚未接入客戶資料"
    >
      <DxColumn data-field="Name" caption="公司名稱" />
      <DxColumn data-field="ContactName" caption="聯絡窗口" />
      <DxColumn data-field="Status" caption="狀態" />
    </DxDataGrid>
  </section>
</template>
