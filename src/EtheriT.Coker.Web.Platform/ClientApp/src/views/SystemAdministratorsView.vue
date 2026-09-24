<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import {
  DxColumn,
  DxDataGrid,
  DxPaging,
  DxSearchPanel
} from "devextreme-vue/data-grid";
import DxSelectBox from "devextreme-vue/select-box";
import ConfirmDialog from "@/components/ConfirmDialog.vue";
import { ApiError } from "@/core/coker";
import {
  addSystemAdministrator,
  fetchSystemAdministrators,
  removeSystemAdministrator
} from "@/services/system-administrator-api";
import type {
  SystemAdministrator,
  SystemAdministratorRoleOption,
  SystemAdministratorUserOption
} from "@/types/system-administrator";

const administrators = ref<SystemAdministrator[]>([]);
const users = ref<SystemAdministratorUserOption[]>([]);
const roles = ref<SystemAdministratorRoleOption[]>([]);
const selectedUserId = ref<number | null>(null);
const selectedRoleId = ref<number | null>(null);
const pendingRemove = ref<SystemAdministrator | null>(null);
const loading = ref(false);
const saving = ref(false);
const pageError = ref("");

const userDisplay = (user: SystemAdministratorUserOption | null): string => user
  ? `${user.Name || "（未填姓名）"}｜${user.Account}${user.Email ? `｜${user.Email}` : ""}`
  : "";

const selectedRole = computed(() => roles.value.find(role => role.Id === selectedRoleId.value));

async function load(): Promise<void> {
  loading.value = true;
  pageError.value = "";
  try {
    const page = await fetchSystemAdministrators();
    administrators.value = page.Administrators;
    users.value = page.Users;
    roles.value = page.Roles;
    if (!roles.value.some(role => role.Id === selectedRoleId.value))
      selectedRoleId.value = roles.value[0]?.Id ?? null;
  }
  catch (error) {
    console.error(error);
    pageError.value = "系統管理者資料載入失敗。";
  }
  finally {
    loading.value = false;
  }
}

async function add(): Promise<void> {
  if (!selectedUserId.value || !selectedRoleId.value || saving.value) return;
  saving.value = true;
  pageError.value = "";
  try {
    await addSystemAdministrator(selectedUserId.value, selectedRoleId.value);
    selectedUserId.value = null;
    await load();
  }
  catch (error) {
    console.error(error);
    pageError.value = error instanceof ApiError ? error.message : "加入系統管理者失敗。";
  }
  finally {
    saving.value = false;
  }
}

async function confirmRemove(): Promise<void> {
  if (!pendingRemove.value || saving.value) return;
  saving.value = true;
  pageError.value = "";
  try {
    await removeSystemAdministrator(pendingRemove.value.MappingId);
    pendingRemove.value = null;
    await load();
  }
  catch (error) {
    console.error(error);
    pageError.value = error instanceof ApiError ? error.message : "移除系統管理者失敗。";
    pendingRemove.value = null;
  }
  finally {
    saving.value = false;
  }
}

onMounted(load);
</script>

<template>
  <section class="page-heading">
    <div>
      <h1>系統管理者</h1>
      <p>管理可登入 Coker Platform 的系統維護角色成員；移除只會解除角色，不會刪除使用者帳號。</p>
    </div>
  </section>

  <p v-if="pageError" class="alert alert-error" role="alert">{{ pageError }}</p>

  <section class="data-card add-card">
    <div class="add-field user-field">
      <label for="administrator-user">使用者</label>
      <DxSelectBox id="administrator-user"
                   v-model:value="selectedUserId"
                   :data-source="users"
                   value-expr="Id"
                   :display-expr="userDisplay"
                   :search-enabled="true"
                   :show-clear-button="true"
                   search-mode="contains"
                   placeholder="搜尋姓名、帳號或 Email" />
    </div>
    <div class="add-field role-field">
      <label for="administrator-role">系統維護角色</label>
      <select id="administrator-role" v-model.number="selectedRoleId">
        <option v-for="role in roles" :key="role.Id" :value="role.Id">
          {{ role.Name }}{{ role.IsSuperUser ? "（總管理者）" : "" }}
        </option>
      </select>
    </div>
    <button class="ui-button ui-button-primary"
            type="button"
            :disabled="saving || !selectedUserId || !selectedRoleId"
            @click="add">
      <span class="material-symbols-outlined">person_add</span>
      加入系統管理者
    </button>
  </section>

  <p v-if="!roles.length && !loading" class="alert alert-warning" role="status">
    目前沒有有效的「系統維護」角色，請先確認 Roles 資料。
  </p>

  <section class="data-card">
    <DxDataGrid :data-source="administrators"
                key-expr="MappingId"
                :show-borders="true"
                :column-auto-width="true"
                :column-hiding-enabled="true"
                :hover-state-enabled="true"
                no-data-text="目前沒有系統管理者">
      <DxSearchPanel :visible="true" :width="280" placeholder="搜尋姓名、帳號、角色" />
      <DxPaging :page-size="20" />
      <DxColumn data-field="Name" caption="姓名" :min-width="140" />
      <DxColumn data-field="Account" caption="帳號" :min-width="150" />
      <DxColumn data-field="Email" caption="Email" :min-width="200" />
      <DxColumn data-field="RoleName" caption="系統角色" :min-width="160" />
      <DxColumn caption="操作" :width="90" :allow-sorting="false" cell-template="actions" />
      <template #actions="{ data }">
        <button class="text-button text-button-danger"
                type="button"
                title="移除管理角色"
                :disabled="data.data.IsCurrentUser"
                @click="pendingRemove = data.data">
          <span class="material-symbols-outlined">person_remove</span>
        </button>
      </template>
    </DxDataGrid>
  </section>

  <ConfirmDialog :open="pendingRemove !== null"
                 icon="person_remove"
                 tone="danger"
                 title="確定要移除系統管理角色嗎？"
                 :message="`將解除「${pendingRemove?.Name ?? ''}」的「${pendingRemove?.RoleName ?? ''}」角色，不會刪除使用者帳號。`"
                 confirm-text="移除角色"
                 :busy="saving"
                 @confirm="confirmRemove"
                 @cancel="pendingRemove = null" />
</template>

<style scoped>
.add-card { display: grid; grid-template-columns: minmax(280px, 1fr) minmax(220px, .55fr) auto; gap: 1rem; align-items: end; padding: 1.25rem; margin-bottom: 1rem; }
.add-field { display: grid; gap: .4rem; }
.add-field label { font-size: .86rem; font-weight: 700; color: #445064; }
.add-field select { height: 38px; padding: 0 .65rem; border: 1px solid #d3d9e3; border-radius: 4px; background: #fff; }
@media (max-width: 900px) { .add-card { grid-template-columns: 1fr; align-items: stretch; } }
</style>
