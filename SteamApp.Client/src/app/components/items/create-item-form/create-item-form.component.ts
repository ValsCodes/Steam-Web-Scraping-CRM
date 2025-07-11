import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ItemService } from '../../../services/item/item.service';
import { CreateItem } from '../../../models/item.model';
import {
  Class,
  classesCollection,
  classesMap,
} from '../../../models/enums/class.enum';
import {
  Slot,
  slotsCollection,
  slotsMap,
} from '../../../models/enums/slot.enum';

@Component({
  selector: 'steam-create-item-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-item-form.component.html',
  styleUrl: './create-item-form.component.scss',
})
export class CreateItemFormComponent implements OnInit {
  itemForm!: FormGroup;

  classes = classesCollection;
  getClassLabel(id: Class) {
    console.log(classesMap[id])
    return classesMap[id];
  }

  slots = slotsCollection;

  getSlotLabel(id: Slot) {
    return slotsMap[id];
  }

  constructor(
    private router: Router,
    private fb: FormBuilder,
    private itemService: ItemService
  ) {}

  ngOnInit(): void {
    this.itemForm = this.fb.group({
      name: ['', Validators.required],
      classId: [null],
      slotId: [null],
      isActive: [true],
      isWeapon: [true],
      currentStock: [null]
    });
  }

  onSubmit(): void {
    if (this.itemForm.invalid) {
      this.itemForm.markAllAsTouched();
      return;
    }

    const newItem: CreateItem = {
      id: 0,
      ...this.itemForm.value,
    };

    this.itemService.createItem(newItem).subscribe({
      next: (item) => {
        console.log('Created item', item);
        this.router.navigate(['items-catalog']);
      },
      error: (err) => console.error('Error creating item:', err),
    });
  }
}
