let reauthenticationTicket = "";

export function setReauthenticationTicket(ticket: string): void {
  reauthenticationTicket = ticket;
}

export function getReauthenticationTicket(): string {
  return reauthenticationTicket;
}
