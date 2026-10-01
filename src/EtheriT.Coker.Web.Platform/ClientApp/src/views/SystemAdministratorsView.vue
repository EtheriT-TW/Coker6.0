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
  removeMvcSystemAdministrator,
  removeSystemAdministrator,
  resendSystemAdministratorInvitation,
  revokeSystemAdministratorInvitation
} from "@/services/system-administrator-api";
import type {
  CreateSystemAdministratorInvitationResponse,
  MvcSystemAdministrator,
  SystemAdministrator,
  SystemAdministratorInvitation,
  SystemAdministratorMvcRoleOption,
  SystemAdministratorRoleOption,
  SystemAdministratorUserOption
} from "@/types/system-administrator";

const administrators = ref<SystemAdministrator[]>([]);
const mvcAdministrators = ref<MvcSystemAdministrator[]>([]);
const users = ref<SystemAdministratorUserOption[]>([]);
const roles = ref<SystemAdministratorRoleOption[]>([]);
const mvcRoles = ref<SystemAdministratorMvcRoleOption[]>([]);
const invitations = ref<SystemAdministratorInvitation[]>([]);
const selectedUserId = ref<number | null>(null);
const selectedRoleId = ref<number | null>(null);
const pendingRemove = ref<SystemAdministrator | null>(null);
const pendingMvcRemove = ref<MvcSystemAdministrator | null>(null);
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
    mvcAdministrators.value = page.MvcAdministrators;
    users.value = page.Users;
    roles.value = page.Roles;
    mvcRoles.value = page.MvcRoles;
    invitations.value = page.Invitations;
    if (!users.value.some(user => user.Id === selectedUserId.value))
      selectedUserId.value = null;
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
    message: `確定核准 ${invitation.Name}（${invitation.Email}）？MVC 系統角色：${invitation.MvcRoleName || "無"}；平台角色：${invitation.PlatformRoleName || "無"}。`,
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

async function confirmMvcRemove(): Promise<void> {
  if (!pendingMvcRemove.value || saving.value) return;
  saving.value = true;
  pageError.value = "";
  try {
    await removeMvcSystemAdministrator(pendingMvcRemove.value.UserId);
    pendingMvcRemove.value = null;
    await load();
  }
  catch (error) {
    console.error(error);
    pageError.value = error instanceof ApiError ? error.message : "取消 MVC 系統管理者身分失敗。";
    pendingMvcRemove.value = null;
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
      <p>MVC 與平台角色可分開授予；新邀請需完成 Email 驗證與管理員核准。</p>
    </div>
    <div class="heading-actions">
      <button class="ui-button ui-button-primary"
              type="button"
              :disabled="loading || (!mvcRoles.length && !roles.length)"
              @click="createDialogOpen = true">
        <span class="material-symbols-outlined">person_add</span>
        邀請使用者
      </button>
    </div>
  </section>

  <p v-if="pageError" class="alert alert-error" role="alert">{{ pageError }}</p>

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
            <td>MVC：{{ invitation.MvcRoleName || "無" }}<small>平台：{{ invitation.PlatformRoleName || "無" }}</small></td>
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

  <section class="data-card mvc-administrators-card">
    <h2>MVC 系統管理者</h2>
    <p>取消身分會解除此人的所有 MVC 系統管理角色；使用者帳號、其他角色及客戶管理平台角色會保留。</p>
    <DxDataGrid :data-source="mvcAdministrators"
                key-expr="UserId"
                :show-borders="true"
                :column-auto-width="true"
                :column-hiding-enabled="true"
                :hover-state-enabled="true"
                no-data-text="目前沒有 MVC 系統管理者">
      <DxSearchPanel :visible="true" :width="280" placeholder="搜尋姓名、帳號、角色" />
      <DxPaging :page-size="20" />
      <DxColumn data-field="Name" caption="姓名" :min-width="140" />
      <DxColumn data-field="Account" caption="帳號" :min-width="150" />
      <DxColumn data-field="Email" caption="Email" :min-width="200" />
      <DxColumn data-field="RoleNames" caption="MVC 系統角色" :min-width="180" />
      <DxColumn caption="操作" :width="90" :allow-sorting="false" cell-template="mvc-actions" />
      <template #mvc-actions="{ data }">
        <button class="text-button text-button-danger"
                type="button"
                title="取消 MVC 管理者身分"
                aria-label="取消 MVC 管理者身分"
                :disabled="loading || saving || data.data.IsCurrentUser"
                @click="pendingMvcRemove = data.data">
          <span class="material-symbols-outlined" aria-hidden="true">person_remove</span>
        </button>
      </template>
    </DxDataGrid>
  </section>

  <section class="data-card platform-administrators-card">
    <h2>客戶管理平台角色</h2>
    <p class="section-description">從上方 MVC 系統管理者中選擇使用者，直接授予平台角色。</p>
    <div class="add-card">
      <div class="add-field user-field">
        <label for="administrator-user">使用者</label>
        <DxSelectBox id="administrator-user"
                     :height="38"
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
        <label for="administrator-role">平台角色</label>
        <select id="administrator-role" v-model.number="selectedRoleId">
          <option v-for="role in roles" :key="role.Id" :value="role.Id">
            {{ role.Name }}
          </option>
        </select>
      </div>
      <button class="ui-button ui-button-primary"
              type="button"
              :disabled="loading || saving || !selectedUserId || !selectedRoleId"
              @click="add">
        <span class="material-symbols-outlined">person_add</span>
        分配角色
      </button>
      <small v-if="selectedRole" class="role-description">{{ selectedRole.Description }}</small>
    </div>

    <p v-if="!roles.length && !loading" class="alert alert-warning" role="status">
      目前沒有有效的客戶管理平台角色，請確認 PlatformRoles Seed 與 Migration 是否已套用。
    </p>
    <DxDataGrid :data-source="administrators"
                key-expr="MappingId"
                :show-borders="true"
                :column-auto-width="true"
                :column-hiding-enabled="true"
                :hover-state-enabled="true"
                no-data-text="目前尚未分配平台角色">
      <DxSearchPanel :visible="true" :width="280" placeholder="搜尋姓名、帳號、角色" />
      <DxPaging :page-size="20" />
      <DxColumn data-field="Name" caption="姓名" :min-width="140" />
      <DxColumn data-field="Account" caption="帳號" :min-width="150" />
      <DxColumn data-field="Email" caption="Email" :min-width="200" />
      <DxColumn data-field="RoleName" caption="平台角色" :min-width="180" />
      <DxColumn caption="操作" :width="90" :allow-sorting="false" cell-template="actions" />
      <template #actions="{ data }">
        <button class="text-button text-button-danger"
                type="button"
                title="移除客戶管理平台角色"
                :disabled="loading || saving || data.data.IsCurrentUser"
                @click="pendingRemove = data.data">
          <span class="material-symbols-outlined">person_remove</span>
        </button>
      </template>
    </DxDataGrid>
  </section>

  <ConfirmDialog :open="pendingRemove !== null"
                 icon="person_remove"
                 tone="danger"
                 title="確定要移除客戶管理平台角色嗎？"
                 :message="`將解除「${pendingRemove?.Name ?? ''}」的「${pendingRemove?.RoleName ?? ''}」角色，不會刪除使用者帳號。`"
                 confirm-text="移除角色"
                 :busy="saving"
                 @confirm="confirmRemove"
                 @cancel="pendingRemove = null" />

  <ConfirmDialog :open="pendingMvcRemove !== null"
                 icon="person_remove"
                 tone="danger"
                 title="確定要取消 MVC 系統管理者身分嗎？"
                 :message="`將解除「${pendingMvcRemove?.Name ?? ''}」的所有 MVC 系統管理角色（${pendingMvcRemove?.RoleNames ?? ''}）。使用者帳號、其他角色及客戶管理平台角色會保留。`"
                 confirm-text="取消管理者身分"
                 :busy="saving"
                 @confirm="confirmMvcRemove"
                 @cancel="pendingMvcRemove = null" />

  <CreateSystemAdministratorDialog :open="createDialogOpen"
                                   :mvc-roles="mvcRoles"
                                   :platform-roles="roles"
                                   @created="handleCreated"
                                   @cancel="createDialogOpen = false" />
</template>

<style scoped>
.mvc-administrators-card, .platform-administrators-card { min-height: 0; margin-bottom: 1.25rem; }
.mvc-administrators-card h2, .platform-administrators-card h2 { margin: 0 0 .5rem; font-size: 1.05rem; }
.mvc-administrators-card p, .section-description { margin: 0 0 1rem; color: #687386; font-size: .86rem; line-height: 1.5; }
.add-card { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, .65fr) auto; gap: .75rem 1rem; align-items: end; margin: 1rem 0 1.25rem; padding: 1rem; background: #f8faff; border: 1px solid #e5eaf3; border-radius: 8px; }
.add-field { display: grid; gap: .4rem; min-width: 0; }
.add-field label { font-size: .86rem; font-weight: 700; color: #445064; }
.add-field select { width: 100%; min-width: 0; height: 38px; padding: 0 .65rem; border: 1px solid #d3d9e3; border-radius: 4px; background: #fff; }
.role-description { grid-column: 1 / -1; color: #687386; line-height: 1.5; }
.add-card > .ui-button { height: 38px; white-space: nowrap; }
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
}
</style>
