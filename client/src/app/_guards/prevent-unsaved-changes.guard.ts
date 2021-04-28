import { Injectable } from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { Observable } from 'rxjs';
import { MembersEditComponent } from '../members/members-edit/members-edit.component';
import { ConfirmService } from '../_services/confirm.service';

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<unknown> {
  constructor(private confirmService: ConfirmService) {}

  canDeactivate(component: MembersEditComponent): Observable<boolean> | boolean {
    if(component.editForm.dirty) {
      return this.confirmService.confirm("Warning", "Are you sure you want continue? Any unsaved changes will be lost.");
    }
    return true;
  }
  
}
