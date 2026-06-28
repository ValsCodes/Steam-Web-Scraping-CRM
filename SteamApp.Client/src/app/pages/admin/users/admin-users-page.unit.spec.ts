import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import {
  AdminUserService,
  AdminUserSummary,
} from '../../../services';
import { AdminUsersPage } from './admin-users-page';

describe('AdminUsersPage', () => {
  let fixture: ComponentFixture<AdminUsersPage>;
  let component: AdminUsersPage;
  let service: jasmine.SpyObj<AdminUserService>;

  const users: AdminUserSummary[] = [
    {
      id: 'admin-1',
      displayName: 'Admin User',
      firstName: 'Admin',
      lastName: 'User',
      userName: 'admin',
      email: 'admin@example.com',
      phone: null,
      roles: ['User', 'Admin'],
      effectiveRole: 'Admin',
      isCurrentUser: true,
    },
    {
      id: 'user-1',
      displayName: 'Normal User',
      firstName: 'Normal',
      lastName: 'User',
      userName: 'normal',
      email: 'user@example.com',
      phone: '+3595550100',
      roles: ['User'],
      effectiveRole: 'User',
      isCurrentUser: false,
    },
  ];

  beforeEach(async () => {
    service = jasmine.createSpyObj<AdminUserService>('AdminUserService', [
      'getUsers',
      'updateRole',
    ]);
    service.getUsers.and.returnValue(of(users));
    service.updateRole.and.returnValue(of({
      ...users[1],
      roles: ['User', 'Admin'],
      effectiveRole: 'Admin',
    }));

    await TestBed.configureTestingModule({
      imports: [AdminUsersPage],
      providers: [
        { provide: AdminUserService, useValue: service },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminUsersPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads and renders users', () => {
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';

    expect(service.getUsers).toHaveBeenCalled();
    expect(text).toContain('Admin User');
    expect(text).toContain('Normal User');
  });

  it('updates a user role and reflects the returned summary', () => {
    component.roleChanged(users[1], 'Admin');

    expect(service.updateRole).toHaveBeenCalledWith('user-1', 'Admin');
    expect(component.users.find(user => user.id === 'user-1')?.effectiveRole).toBe('Admin');
    expect(component.statusMessage).toBe('Normal User is now Admin');
  });

  it('disables role changes for the current admin user', () => {
    expect(component.canChangeRole(users[0])).toBeFalse();
    expect(component.canChangeRole(users[1])).toBeTrue();
  });
});
