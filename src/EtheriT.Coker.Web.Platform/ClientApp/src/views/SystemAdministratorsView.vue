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
import CreateSystemAdministratorDialog from "@/components/CreateSystemAdministratorDialog.vue";
import { ApiError, requestAlert, requestConfirm } from "@/core/coker";
import {
  addSystemAdministrator,
  approveSystemAdministratorInvitation,
  fetchSystemAdministrators,
  removeSystemAdministrator,
  resendSystemAdministratorInvitation,
  revokeSystemAdministratorInvitation
} from "@/services/system-administrator-api";
import type {
  CreateSystemAdministratorInvitationResponse,
  SystemAdministrator,
  SystemAdministratorInvitation,
  SystemAdministratorMvcRoleOption,
  SystemAdministratorRoleOption,
  SystemAdministratorUserOption
} from "@/types/system-administrator";

const administrators = ref<SystemAdministrator[]>([]);
const users = ref<SystemAdministratorUserOption[]>([]);
const roles = ref<SystemAdministratorRoleOption[]>([]);
const mvcRoles = ref<SystemAdministratorMvcRoleOption[]>([]);
const invitations = ref<SystemAdministratorInvitation[]>([]);
const selectedUserId = ref<number | null>(null);
const selectedRoleId = ref<number | null>(null);
const pendingRemove = ref<SystemAdministrator | null>(null);
const loading = ref(false);
const saving = ref(false);
const pageError = ref("");
const createDialogOpen = ref(false);
const invitationBusyId = ref<number | null>(null);

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
    mvcRoles.value = page.MvcRoles;
    invitations.value = page.Invitations;
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

async function handleCreated(response: CreateSystemAdministratorInvitationResponse): Promise<void> {
  createDialogOpen.value = false;
  await load();
  await requestAlert({
    title: response.EmailSent ? "邀請已寄出" : "邀請已建立，但信件未寄出",
    message: response.EmailSent
      ? `已寄送帳號啟用信至 ${response.Invitation.Email}。`
      : response.EmailError ?? "請稍後從待核准清單重新寄送。",
    tone: response.EmailSent ? "primary" : "danger"
  });
}

function invitationStatus(invitation: SystemAdministratorInvitation): string {
  if (invitation.EmailVerifiedAtUtc) return "等待管理員核准";
  return new Date(invitation.ExpiresAtUtc).getTime() < Date.now() ? "邀請已到期" : "等待 Email 驗證";
}

async function resendInvitation(invitation: SystemAdministratorInvitation): Promise<void> {
  if (invitationBusyId.value !== null) return;
  invitationBusyId.value = invitation.Id;
  try {
    await resendSystemAdministratorInvitation(invitation.Id);
    await load();
    await requestAlert({ title: "邀請已重新寄出", message: `已重新寄送帳號啟用信至 ${invitation.Email}。` });
  }
  catch (error) {
    console.error(error);
    await requestAlert({ title: "重新寄送失敗", message: error instanceof ApiError ? error.message : "請稍後再試。" });
  }
  finally { invitationBusyId.value = null; }
}

async function approveInvitation(invitation: SystemAdministratorInvitation): Promise<void> {
  const confirmed = await requestConfirm({
    title: "核准管理員權限",
    message: `確定將 ${invitation.Name}（${invitation.Email}）核准為「${invitation.MvcRoleName}」及「${invitation.PlatformRoleName}」？`,
    icon: "verified_user",
    confirmText: "核准"
  });
  if (!confirmed || invitationBusyId.value !== null) return;

  invitationBusyId.value = invitation.Id;
  try {
    await approveSystemAdministratorInvitation(invitation.Id);
    await load();
  }
  catch (error) {
    console.error(error);
    await requestAlert({ title: "核准失敗", message: error instanceof ApiError ? error.message : "請稍後再試。" });
  }
  finally { invitationBusyId.value = null; }
}

async function revokeInvitation(invitation: SystemAdministratorInvitation): Promise<void> {
  const confirmed = await requestConfirm({
    title: "撤銷管理員邀請",
    message: `撤銷後，${invitation.Email} 的啟用連結會立即失效，且不會取得任何管理權限。`,
    icon: "person_cancel",
    confirmText: "撤銷邀請",
    tone: "danger"
  });
  if (!confirmed || invitationBusyId.value !== null) return;

  invitationBusyId.value = invitation.Id;
  try {
    await revokeSystemAdministratorInvitation(invitation.Id);
    await load();
  }
  catch (error) {
    console.error(error);
    await requestAlert({ title: "撤銷失敗", message: error instanceof ApiError ? error.message : "請稍後再試。" });
  }
  finally { invitationBusyId.value = null; }
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
      <p>邀請新的管理者，或替既有 MVC 系統管理者分配 Platform 專用角色；新邀請需完成 Email 驗證與管理員核准。</p>
    </div>
    <div class="heading-actions">
      <button class="ui-button ui-button-primary"
              type="button"
              :disabled="loading || !roles.length || !mvcRoles.length"
              @click="createDialogOpen = true">
        <span class="material-symbols-outlined">person_add</span>
        邀請使用者
      </button>
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
                   placeholder="搜尋 MVC 系統管理者" />
    </div>
    <div class="add-field role-field">
      <label for="administrator-role">Platform 角色</label>
      <select id="administrator-role" v-model.number="selectedRoleId">
        <option v-for="role in roles" :key="role.Id" :value="role.Id">
          {{ role.Name }}
        </option>
      </select>
      <small v-if="selectedRole" class="role-description">{{ selectedRole.Description }}</small>
    </div>
    <button class="ui-button ui-button-primary"
            type="button"
            :disabled="saving || !selectedUserId || !selectedRoleId"
            @click="add">
      <span class="material-symbols-outlined">person_add</span>
      分配 Platform 角色
    </button>
  </section>

  <p v-if="!roles.length && !loading" class="alert alert-warning" role="status">
    目前沒有有效的 Platform 角色，請確認 PlatformRoles Seed 與 Migration 是否已套用。
  </p>

  <section v-if="invitations.length" class="data-card invitation-card">
    <header>
      <div>
        <h2>待完成的管理員邀請</h2>
        <p>Email 驗證只確認信箱持有人；完成後仍需由總管理者核對資料並正式核准。</p>
      </div>
      <span>{{ invitations.length }} 筆</span>
    </header>
    <div class="table-wrap">
      <table>
        <thead><tr><th>姓名／Email</th><th>預定角色</th><th>帳號</th><th>狀態</th><th>操作</th></tr></thead>
        <tbody>
          <tr v-for="invitation in invitations" :key="invitation.Id">
            <td><strong>{{ invitation.Name }}</strong><small>{{ invitation.Email }}</small></td>
            <td>{{ invitation.MvcRoleName }}<small>{{ invitation.PlatformRoleName }}</small></td>
            <td>{{ invitation.Account || "尚未設定" }}</td>
            <td><span class="invitation-status" :class="{ verified: invitation.EmailVerifiedAtUtc }">{{ invitationStatus(invitation) }}</span></td>
            <td>
              <div class="invitation-actions">
                <button v-if="!invitation.EmailVerifiedAtUtc" class="text-button" type="button" :disabled="invitationBusyId !== null" @click="resendInvitation(invitation)">重新寄送</button>
                <button v-if="invitation.EmailVerifiedAtUtc" class="text-button" type="button" :disabled="invitationBusyId !== null" @click="approveInvitation(invitation)">核准</button>
                <button class="text-button text-button-danger" type="button" :disabled="invitationBusyId !== null" @click="revokeInvitation(invitation)">撤銷</button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>

  <section class="data-card">
    <DxDataGrid :data-source="administrators"
                key-expr="MappingId"
                :show-borders="true"
                :column-auto-width="true"
                :column-hiding-enabled="true"
                :hover-state-enabled="true"
                no-data-text="目前尚未分配 Platform 角色">
      <DxSearchPanel :visible="true" :width="280" placeholder="搜尋姓名、帳號、角色" />
      <DxPaging :page-size="20" />
      <DxColumn data-field="Name" caption="姓名" :min-width="140" />
      <DxColumn data-field="Account" caption="帳號" :min-width="150" />
      <DxColumn data-field="Email" caption="Email" :min-width="200" />
      <DxColumn data-field="RoleName" caption="Platform 角色" :min-width="180" />
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

  <CreateSystemAdministratorDialog :open="createDialogOpen"
                                   :mvc-roles="mvcRoles"
                                   :platform-roles="roles"
                                   @created="handleCreated"
                                   @cancel="createDialogOpen = false" />
</template>

<style scoped>
.add-card { display: grid; grid-template-columns: minmax(280px, 1fr) minmax(220px, .55fr) auto; gap: 1rem; align-items: start; min-height: 0; padding: 1.25rem; margin-bottom: 1rem; }
.add-field { display: grid; gap: .4rem; }
.add-field label { font-size: .86rem; font-weight: 700; color: #445064; }
.add-field select { height: 38px; padding: 0 .65rem; border: 1px solid #d3d9e3; border-radius: 4px; background: #fff; }
.role-description { color: #687386; line-height: 1.45; }
.add-card > .ui-button { margin-top: 1.45rem; }
.invitation-card { margin-bottom: 1rem; padding: 1.25rem; }
.invitation-card > header { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; margin-bottom: 1rem; }
.invitation-card h2 { margin: 0 0 .3rem; font-size: 1.05rem; }
.invitation-card p { margin: 0; color: #687386; font-size: .86rem; }
.invitation-card td small { display: block; margin-top: .25rem; color: #687386; }
.invitation-status { display: inline-flex; padding: .25rem .55rem; border-radius: 999px; background: #fff7ed; color: #9a3412; font-size: .8rem; font-weight: 700; }
.invitation-status.verified { background: #ecfdf5; color: #047857; }
.invitation-actions { display: flex; align-items: center; gap: .65rem; white-space: nowrap; }
@media (max-width: 900px) {
  .add-card { grid-template-columns: 1fr; align-items: stretch; }
  .add-card > .ui-button { margin-top: 0; }
}
</style>
